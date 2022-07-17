using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PokeApiNet; //nuget package for all pokemon
using Type = PokeApiNet.Type;

namespace Pokemon_Weakness_Finder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the name of the pokemon: ");
            string pkmnUser = Console.ReadLine();

            _ = Pkmn(pkmnUser);

            Console.ReadLine();
        }

        public static async Task Pkmn(string userPkmn)
        {
            PokeApiClient pokeClient = new PokeApiClient(); //creating an instance of the API

            Pokedex pokedex = await pokeClient.GetResourceAsync<Pokedex>("national");
            int totalPkmn = pokedex.PokemonEntries.Count; //898! The last listed pokemon is calyrex
            int checker = 0;

            //Checking for a valid Pokemon name entry
            while (checker < totalPkmn) {
                if (pokedex.PokemonEntries[checker].PokemonSpecies.Name.Contains(userPkmn)) {
                    break;
                }
                checker++;
            }

            if (checker == 898) {
                Console.Write("\nYou did not enter a valid Pokemon name.");
                Console.Write("\nProgram is closing.");
                Console.ReadLine();
                System.Environment.Exit(0);
            }

            //List of all pokemon typings... will be used for neutral damage checking
            List<string> allPkmnTypes = new List<string>();
            string[] alltypes = {"normal", "fighting", "flying", "poison", "ground", "rock", "bug", "ghost", "steel",
                "fire", "water", "grass", "electric", "psychic", "ice", "dragon", "dark", "fairy"};
            for (int p = 0; p < alltypes.Length; p++)
            {
                allPkmnTypes.Add(alltypes[p]);
            }

            //Getting the pokemon from the user
            Pokemon pkmn = await pokeClient.GetResourceAsync<Pokemon>(userPkmn);
            string pkmnName = pkmn.Name.Substring(0, 1).ToUpper() + pkmn.Name.Substring(1);
            string pkmntype1 = pkmn.Types[0].Type.Name;  //holder for the 1st type
            string pkmntype2 = ""; //holder for the 2nd type if needed

            int checkForMultipleType = pkmn.Types.Count;

            if (checkForMultipleType > 1)
                pkmntype2 = pkmn.Types[1].Type.Name; 
            else
                pkmntype2 = null;


            //TYPE 1
            Type type1 = await pokeClient.GetResourceAsync<Type>(pkmntype1); //rock
            int type1ID = type1.Id;

            //weakness
            int t1_2xWeakCount = type1.DamageRelations.DoubleDamageFrom.Count;
            string[] t1_2xWeakArr = new string[t1_2xWeakCount];
            for (int w1 = 0; w1 < t1_2xWeakCount; w1++)
            {
                t1_2xWeakArr[w1] = type1.DamageRelations.DoubleDamageFrom[w1].Name;
            }
            List<string> t1doubleWeak = t1_2xWeakArr.ToList();

            //resistance
            int t1_2xResCount = type1.DamageRelations.HalfDamageFrom.Count;
            string[] t1_2xResArr = new string[t1_2xResCount];
            for (int r1 = 0; r1 < t1_2xResCount; r1++)
            {
                t1_2xResArr[r1] = type1.DamageRelations.HalfDamageFrom[r1].Name;
            }
            List<string> t1doubleRes = t1_2xResArr.ToList();

            //immunity
            int t1_nullCount = type1.DamageRelations.NoDamageFrom.Count;
            string[] t1_nullArr = new string[t1_nullCount];
            for (int r1 = 0; r1 < t1_nullCount; r1++)
            {
                t1_nullArr[r1] = type1.DamageRelations.NoDamageFrom[r1].Name;
            }
            List<string> t1null = t1_nullArr.ToList();

            //neutral
            List<string> typingsUsed_single = new List<string>();
            typingsUsed_single = t1doubleWeak.Concat(t1doubleRes).Concat(t1null).ToList();

            List<string> neutralSingle = new List<string>();
            neutralSingle.AddRange(allPkmnTypes);

            foreach (var nitem in typingsUsed_single)
            {
                if (typingsUsed_single.Contains(nitem) && neutralSingle.Contains(nitem))
                {
                    neutralSingle.Remove(nitem);
                }
            }

            //TYPE 2
            if (pkmntype2 != null)
            {
                Type type2 = await pokeClient.GetResourceAsync<Type>(pkmntype2); 
                int type2ID = type2.Id;

                //weakness
                int t2_2xWeakCount = type2.DamageRelations.DoubleDamageFrom.Count;
                string[] t2_2xWeakArr = new string[t2_2xWeakCount];
                for (int w2 = 0; w2 < t2_2xWeakCount; w2++)
                {
                    t2_2xWeakArr[w2] = type2.DamageRelations.DoubleDamageFrom[w2].Name;
                }
                List<string> t2doubleWeak = t2_2xWeakArr.ToList();

                //resistance
                int t2_2xResCount = type2.DamageRelations.HalfDamageFrom.Count;
                string[] t2_2xResArr = new string[t2_2xResCount];
                for (int r2 = 0; r2 < t2_2xResCount; r2++)
                {
                    t2_2xResArr[r2] = type2.DamageRelations.HalfDamageFrom[r2].Name;
                }
                List<string> t2doubleRes = t2_2xResArr.ToList();

                //CHECKING FOR QUAD WEAKNESS & RESISTANCE

                //weakness
                List<string> quadWeak = new List<string>();
                foreach (var witem in t1doubleWeak)
                {
                    if (t2doubleWeak.Contains(witem))
                    {
                        quadWeak.Add(witem); //adding the quad weaknesses to one list
                    }
                }
                foreach (var witem in quadWeak) //removing the types that the pkmn is quad weak to from the 2x weakness lists
                {
                    if (t2doubleWeak.Contains(witem) && t1doubleWeak.Contains(witem))
                    {
                        t1doubleWeak.Remove(witem);
                        t2doubleWeak.Remove(witem);
                    }
                }

                List<string> doubleWeak = new List<string>(); //adding the remains of the 2x weakness to one list
                foreach (var witem in t1doubleWeak)
                {
                    doubleWeak.Add(witem);
                }
                foreach (var witem in t2doubleWeak)
                {
                    doubleWeak.Add(witem);
                }

                //resistance
                List<string> quadRes = new List<string>();
                foreach (var ritem in t1doubleRes)
                {
                    if (t2doubleRes.Contains(ritem))
                    {
                        quadRes.Add(ritem); //adding the quad resistances to one list
                    }
                }
                foreach (var ritem in quadRes) //removing the types that the pkmn is quad resistant to from the 2x resistance lists
                {
                    if (t2doubleRes.Contains(ritem) && t1doubleRes.Contains(ritem))
                    {
                        t1doubleRes.Remove(ritem);
                        t2doubleRes.Remove(ritem);
                    }
                }

                List<string> doubleRes = new List<string>(); //adding the remains of the 2x resistances to one list
                foreach (var ritem in t1doubleRes)
                {
                    doubleRes.Add(ritem);
                }
                foreach (var ritem in t2doubleRes)
                {
                    doubleRes.Add(ritem);
                }

                //immunties
                int t2_nullCount = type2.DamageRelations.NoDamageFrom.Count;
                string[] t2_nullArr = new string[t2_nullCount];
                for (int i2 = 0; i2 < t2_nullCount; i2++)
                {
                    t2_nullArr[i2] = type2.DamageRelations.NoDamageFrom[i2].Name;
                }
                List<string> t2null = t2_nullArr.ToList();

                List<string> immunity = new List<string>();

                immunity = t1null.Concat(t2null).ToList();

                //neutral
                List<string> typingsUsed_double = new List<string>();
                typingsUsed_double = doubleRes.Concat(quadRes).Concat(doubleWeak).Concat(quadWeak).Concat(immunity).ToList();

                List<string> neutralDouble = new List<string>();
                neutralDouble.AddRange(allPkmnTypes);

                foreach (var nitem in typingsUsed_double)
                {
                    if (typingsUsed_double.Contains(nitem) && neutralDouble.Contains(nitem))
                    {
                        neutralDouble.Remove(nitem);
                    }
                }

                //Printing Area for 2 types
                Console.WriteLine("\n{0}'s type is = {1} {2}", pkmnName, pkmntype1, pkmntype2);
                Console.WriteLine("Type 1 ID is {0} and Type 2 ID is {1}", type1ID, type2ID);

                Console.Write("\n{0} takes 1/2 damage from: ", pkmnName);
                foreach (var x in doubleRes)
                    Console.Write("{0} ", x);

                Console.Write("\n{0} takes 1/4 damage from: ", pkmnName);
                foreach (var x in quadRes)
                    Console.Write("{0} ", x);

                Console.Write("\n{0} takes 2x damage from: ", pkmnName);
                foreach (var x in doubleWeak)
                    Console.Write("{0} ", x);

                Console.Write("\n{0} takes 4x damage from: ", pkmnName);
                foreach (var y in quadWeak)
                    Console.Write("{0} ", y);

                Console.Write("\n{0} takes No damage from: ", pkmnName);
                foreach (var y in immunity)
                    Console.Write("{0} ", y);
                
                Console.Write("\n{0} takes Neutral damage from: ", pkmnName);
                foreach (var y in neutralDouble)
                    Console.Write("{0} ", y);
                
            }

            else if (pkmntype2 == null)
            {
                //Printing Area for only 1 type
                Console.WriteLine("\n{0}'s type is = {1}", pkmnName, pkmntype1);//rock ground
                Console.WriteLine("Type 1 ID is {0}", type1ID);

                Console.Write("\n{0} takes 1/2 damage from: ", pkmnName);
                foreach (var x in t1doubleRes)
                    Console.Write("{0} ", x);

                Console.Write("\n{0} takes 2x damage from: ", pkmnName);
                foreach (var x in t1doubleWeak)
                    Console.Write("{0} ", x);

                Console.Write("\n{0} takes No damage from: ", pkmnName);
                foreach (var x in t1null)
                    Console.Write("{0} ", x);

                Console.Write("\n{0} takes Neutral damage from: ", pkmnName);
                foreach (var x in neutralSingle)
                    Console.Write("{0} ", x);
            }
            Console.WriteLine();
        }
    }
}