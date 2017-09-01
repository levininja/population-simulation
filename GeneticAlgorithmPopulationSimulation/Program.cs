using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GeneticAlgorithmPopulationSimulation
{
	class Program
	{
		//Herd stabilization
		private static int numberOfAnimals = 800;
		private static int stabilizingNumber = 800;
		//Herd survival
		private static int numberOfAnimalsWithSuperiorTrait = 1;
		private static double normalOddsOfSurvival = .50;
		private static double superiorTraitBoost = .10;
		//Iterations
		//Woolly Mammoths existed for 400,000 years and they lived 60 years, so maybe had young at 20 or 15.
		//400,000 / 20 = 20,000 generations.  So 10,000 iterations represents half of their entire history.
		//400,000 / 15 = 26,667 generations.  So 14,000 iterations represents half of their entire history.
		private static int numberOfIterations = 2000;

		public static double getOddsOfSurvival(bool hasDesirableTrait, double adjustmentPercentagePoints)
		{
			double oddsOfSurvival = normalOddsOfSurvival + adjustmentPercentagePoints / 100;
			if (oddsOfSurvival > .70)
				oddsOfSurvival = .70;
			if (oddsOfSurvival < .30)
				oddsOfSurvival = .30;
			oddsOfSurvival = hasDesirableTrait ? oddsOfSurvival + superiorTraitBoost : oddsOfSurvival;
			return oddsOfSurvival;
		}
		private static double getDeathProbabilityOfHumanMales(int age)
		{
			double firstOrder = (2.62 / 1000 / 1000 / 1000 / 1000 / 1000) * Math.Pow(age, 7);
			double secondOrder = .08	* Math.Pow(1.20, Math.Pow(age - 100, 2) / -20);
			double thirdOrder = .004	* Math.Pow(1.09, Math.Pow(age - 56, 2) / -20);
			double fourthOrder = .001	* Math.Pow(1.15, Math.Pow(age - 26, 2) / -10);
			double fifthOrder = .00002 * age;
			return firstOrder + secondOrder + thirdOrder + fourthOrder + fifthOrder;
		}

		static void Main(string[] args)
		{
			Console.WriteLine("Welcome to the genetic algorithm simulator for a population of animals.");
			Console.WriteLine("Iterations: {2}.  In each iteration, population: {0}.  Number with superior trait: {1}.",
					numberOfAnimals, numberOfAnimalsWithSuperiorTrait, numberOfIterations);

			Random randomNumberGenerator = new Random();
			int numberOfSuccesses = 0;
			int numberOfFailures = 0;
			int numberOfIndeterminate = 0;

			Console.WriteLine("How many super-iterations would you like to run?");
			string result = Console.ReadLine();
			int numberOfSuperIterations = Convert.ToInt32(result);

			for (int i = 0; i < numberOfSuperIterations; i++)
			{
				Console.WriteLine("Running {0} iterations...", numberOfIterations);
					
				//Initiate herd
				List<Elephant> herd = new List<Elephant>();
				herd = new List<Elephant>();
				for (int y = 1; y < numberOfAnimals + 1; y++)
				{
					if (y <= numberOfAnimalsWithSuperiorTrait)
						herd.Add(new Elephant(true));
					else
						herd.Add(new Elephant(false));
				}

				for (int x = 0; x < numberOfIterations; x++)
				{
					//Determine new odds of survival
					double diff = herd.Count - stabilizingNumber;
					double adjustmentPercentagePoints = diff < 0 ? Math.Pow(diff, 2) / 500 : 0 - Math.Pow(diff, 2) / 500;
					double normalOddsOfSurvivalThisRound = getOddsOfSurvival(false, adjustmentPercentagePoints);

					//Each member of the herd could die
					for (int m = 0; m < herd.Count; m++)
					{
						if (herd[m].hasDesirableTrait)
						{
							bool thing = true;
						}
						double oddsOfSurvival = getOddsOfSurvival(herd[m].hasDesirableTrait, adjustmentPercentagePoints);
						if (!herd[m].Survives(oddsOfSurvival, randomNumberGenerator))
						{
							herd.RemoveAt(m);
							m--;
						}
					}

					//Remaining members reproduce
					int numberOfTimesToShuffle = randomNumberGenerator.Next(1, 11);
					for(int n = 0;n<numberOfTimesToShuffle; n++)
						ListExtensions.Shuffle(herd, randomNumberGenerator);

					int litterSize = 2;
					List<Elephant> babyElephants = new List<Elephant>();
					for (int d = 0; d < herd.Count - 1; d = d + 2)
					{
						//They have a number of babies according to the species litter size
						//TODO: refine with chance of pregnancy and litter size can be a double.
						for (int c = 0; c < litterSize; c++)
						{
							bool getsTraitFromFirstParent = randomNumberGenerator.Next(0, 2) == 0;
							bool thisBabyHasDesirableTrait = getsTraitFromFirstParent ? herd[d].hasDesirableTrait
								: herd[d + 1].hasDesirableTrait;
							babyElephants.Add(new Elephant(thisBabyHasDesirableTrait));
						}
					}

					//Babies grow up
					herd.AddRange(babyElephants);
				}

				//Calculate number with superior trait and inform user if herd died
				int numberWithSuperiorTrait = 0;
				foreach (Elephant elephant in herd)
					if (elephant.hasDesirableTrait)
						numberWithSuperiorTrait++;
				double percentage = 0;
				if (herd.Count == 0)
					Console.WriteLine("Herd eliminated!");
				else
				{
					percentage = (100 * numberWithSuperiorTrait / herd.Count);
					if (percentage > 90)
						numberOfSuccesses++;
					else if (percentage < 10)
					{
						numberOfFailures++;
					}
					else
					{
						numberOfIndeterminate++;
					}
					Console.WriteLine("{3} iterations complete.  Population of {0} has {1} with desirable trait; {2}%.",
						herd.Count, numberWithSuperiorTrait, percentage.ToString(),
						numberOfIterations.ToString());
				}
			}
			int superIts = Convert.ToInt32(result);
			Console.WriteLine("{0} super-iterations run.", superIts);
			Console.WriteLine("{0} were successes, or {1}%.", numberOfSuccesses, 100 * numberOfSuccesses / superIts);
			Console.WriteLine("{0} were failures, or {1}%.", numberOfFailures, 100 * numberOfFailures / superIts);
			Console.WriteLine("{0} were indeterminate, or {1}%.", numberOfIndeterminate, 100 * numberOfIndeterminate / superIts);
			Console.WriteLine();

			Console.WriteLine("Simulation complete; press anything to exit.");
			Console.ReadLine();
		}
	}
	class Elephant
	{
		public bool hasDesirableTrait { get; set; }

		public Elephant(bool hasDesirableTrait)
		{
			this.hasDesirableTrait = hasDesirableTrait;
		}
		public bool Survives(double oddsOfSurvival, Random randomNumberGenerator)
		{
			double number = randomNumberGenerator.NextDouble();
			bool survived = number < oddsOfSurvival;
			return survived;
		}
	}
	internal static class ListExtensions
	{
		/// <summary>
		/// Fisher-Yates shuffle
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void Shuffle<T>(this IList<T> list, Random rng)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
		/// <summary>
		/// Supposed to be better but it randomly hitches
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void Shuffle<T>(this IList<T> list)
		{
			RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
			int n = list.Count;
			while (n > 1)
			{
				byte[] box = new byte[1];
				do provider.GetBytes(box);
				while (!(box[0] < n * (Byte.MaxValue / n)));
				int k = (box[0] % n);
				n--;
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
