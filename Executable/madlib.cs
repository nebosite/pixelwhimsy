using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace PixelWhimsy
{
	/// <summary>
	/// Summary description for MadLib.
	/// </summary>
	public partial class MadLib
	{
		#region helper classes
        // *********************************************************************
        /// <summary>
        /// There is a bug in the clr random class that this works aroung
        /// </summary>
        // *********************************************************************
        private static class Rand
        {
			private static Random randomGenerator = new Random();
            public static int Next(int maxValue)
            {
                return randomGenerator.Next(maxValue) % maxValue;
            }
        }

		// *********************************************************************
		/// <summary>
		/// The Identity class helps keep track of sentence information 
		/// </summary>
		// *********************************************************************
		private class Identity
		{
			private bool isPlural = false;
			private int tense = 0;
            private bool isReserved = false;
            private bool isInfinitive = false;
            private bool isProgressive = false;
            public Hashtable words = new Hashtable();

			public Identity()
			{
				Clear();
			}

            public bool IsPlural { get { return isPlural; } set { isPlural = value; } }
            public bool IsReserved { get { return isReserved; } set { isReserved = value; } }
            public bool IsInfinitive { get { return isInfinitive; } set { isInfinitive = value; } }
            public bool IsProgressive { get { return isProgressive; } set { isProgressive = value; } }
            public int Tense
            {
                get { return tense; }
                set { tense = value; } 			
}

			public void Clear() 
			{
				isPlural = random.Next(2) == 0;
				tense = random.Next(3);
				words.Clear();
			}

			public void Attach(string type,string word)
			{
				words[type.ToUpper()] = word;
			}

			public string Word(string type)
			{
				string returnMe = (string)words[type];
				if(returnMe == null) returnMe = "";
				return returnMe;
			}

		}
		#endregion

		#region fields, enums and properties

		private Identity[] identities = new Identity[256];
		static private Random random = new Random();
		private string randomName;  
        private int currentIdentity = 255;

		#endregion

		#region sentence templates
		// modifiers in order:  (infinitive ~) (plurality *[0,1]) (Tense: ^[0-2] 0=past, 1=present, 2=future) (Progressive Tense: !) (Identity:  .[0-255]) 
		public static readonly string[] MadlibSentance = new string[] 
		{
			"@ARTICLE.1 @ADJECTIVE @NOUNANIMAL.1 @BE.1 @ADJECTIVE @PUNCTUATION",	 // The white mouse was tired.
			"@VERBTRANSITIVE~*1^1 @ARTICLE.1 @NOUN.1 @PREPOSITION @ARTICLE.2 @ADJECTIVE @NOUN.2 @PUNCTUATION",  // Find the bottlenecks in this ridiculous process.
			"@NOUNSUBJECTIVEPERSONALPRONOUN.1 @VERBTRANSITIVE.1 @ARTICLE.2 @NOUN.2 @PUNCTUATION", // I ate the sandbox
            "@NOUNSUBJECTIVEPERSONALPRONOUN.1 @VERBINTRANSITIVE.1^0 @PREPOSITION @NAMEPLACE @VERBTRANSITIVE^2! @ADJECTIVE @NOUN*1 @PUNCTUATION", // They lived in Redmond making proactive software.
			"@NAMEFIRSTNAME.1 and @NAMEFIRSTNAME.2 went @PREPOSITION the @NOUNPLACE*0 to @VERBTRANSITIVE~^1.1 a @NOUN*0 of @NOUN*1 @PUNCTUATION", // Jack and Jill...
            "@NOUNSUBJECTIVEPERSONALPRONOUN @VERBTRANSITIVE^0 @INTERROGATIVE @NAMEFIRSTNAME @VERBINTRANSITIVE^0 @PREPOSITION @ARTICLE.1 @NOUN.1 @PUNCTUATION",  //No one knew how Mr. Sticky got in the fish tank.

            "@NOUNSUBJECTIVEPERSONALPRONOUN.1 @ADVERB @VERBINTRANSITIVE^1!.1 @PREPOSITION @PUNCTUATION",  //They were always falling down. 
            "@NAMEFIRSTNAME.2*0 , @ARTICLE.1*0 @ADJECTIVE @ADJECTIVE @NOUN.1 , @VERBINTRANSITIVE^1!.2 @PREPOSITION @ARTICLE.2 @ADJECTIVE @NOUN.2 @PUNCTUATION",  //Gerry, the fat orange goldfish, was dozing inside the stone archway.
            "@ARTICLE.1 @NOUN*1.1 @PREPOSITION @NOUNPOSSESSIVEPERSONALPRONOUN.2*0 @NOUN*0 said @NOUNSUBJECTIVEPERSONALPRONOUN.3 @VERBINTRANSITIVE^0.3 @ARTICLE.3 @ADJECTIVE @NOUN.3 @PUNCTUATION", //The girls in her class said he seemed an ideal pet. 
            "@PREPOSITION @NOUNPLACE @ARTICLE*0 @NOUN*0 , @NAMEFIRSTNAME*0.1 @VERBTRANSITIVE^0.1 @ARTICLE.2 @NOUN.2 @PUNCTUATION",  //At school that day, Abby drew an elephant.
            //She tried to imagine what it must be like to have to hang on to things all day 
            //"I think he's grown a bit," Abby told her Mum at breakfast the next day. 
            //That evening Abby went up to her bedroom to check the tank.
        };

		public static readonly string[] MadlibName = new string[] 
		{
			"@NAMEFULLNAME"
		};                                  

		#endregion

		#region words
		// B******************************************************************

		private static readonly string[] punctuation = new string[]
		{
			".",".",".",".",".",".",".",".",".",".",".",".",".","?","!"
		};

		private static readonly string[] preposition = new string[]
		{
            "about",
            "above",
            "across",
            "after",
            "against",
            "along",
            "among",
            "around",
            "at",
            "before",
            "behind",
            "below",
            "beneath",
            "beside",
            "beside",
            "between",
            "beyond",
            "by",
            "despite",
            "down",
            "during",
            "for",
            "from",
            "in front of",
            "in",
            "inside",
            "into",
            "like",
            "near",
            "of",
            "off",
            "on",
            "onto",
            "out",
            "outside",
            "over",
            "past",
            "since",
            "through",
            "throughout",
            "till",
            "to",
            "toward",
            "under",
            "underneath",
            "until",
            "up",
            "upon",
            "with",
            "within",
            "without"
		};

		private static readonly string[] modalVerb = new string[]
		{
			"could", 
			"couldn't", 
			"must", 
			"mustn't", 
			"shall", 
			"shant",
			"should", 
			"shouldn't", 
			"would", 
			"wouldn't", 
			"will",
            "won't",
            "may",
            "might"
		};

		private static readonly string[] singleArticle = new string[] 
		{
			"a",
			"a singular", 
			"about one", 
			"her", 
			"his", 
			"just one", 
			"my", 
			"one", 
			"the",
			"their",
			"this",
			"that",
			"your", 
		};

		private static readonly string[] pluralArticle = new string[] 
		{
			"four", 
			"many",
			"a million", 
			"one thousand", 
			"quite a few", 
			"seven", 
			"some", 
			"thirteen", 
			"those",
			"three", 
			"twenty two", 
			"two", 
		};

		private static readonly string[] interrogative = new string[] 
		{
			"what",
            "who",
            "where",
            "when",
            "why",
            "how"
		};

        #region ADVERBS
        private static readonly string[] adverb = new string[]
        {
            "accidentally",
            "afterwards",
            "almost",
            "always",
            "angrily",
            "annually",
            "anxiously",
            "awkwardly",
            "badly",
            "blindly",
            "boastfully",
            "boldly",
            "bravely",
            "briefly",
            "brightly",
            "busily",
            "calmly",
            "carefully",
            "carelessly",
            "cautiously",
            "cheerfully",
            "clearly",
            "correctly",
            "courageously",
            "crossly",
            "cruelly",
            "daily",
            "defiantly",
            "deliberately",
            "doubtfully",
            "easily",
            "elegantly",
            "enormously",
            "enthusiastically",
            "equally",
            "even",
            "eventually",
            "exactly",
            "faithfully",
            "far",
            "fast",
            "fatally",
            "fiercely",
            "fondly",
            "foolishly",
            "fortunately",
            "frantically",
            "gently",
            "gladly",
            "gracefully",
            "greedily",
            "happily",
            "hastily",
            "honestly",
            "hourly",
            "hungrily",
            "innocently",
            "inquisitively",
            "irritably",
            "joyously",
            "justly",
            "kindly",
            "lazily",
            "less",
            "loosely",
            "loudly",
            "madly",
            "merrily",
            "monthly",
            "more",
            "mortally",
            "mysteriously",
            "nearly",
            "neatly",
            "nervously",
            "never",
            "noisily",
            "not",
            "obediently",
            "obnoxiously",
            "often",
            "only",
            "painfully",
            "perfectly",
            "politely",
            "poorly",
            "powerfully",
            "promptly",
            "punctually",
            "quickly",
            "quietly",
            "rapidly",
            "rarely",
            "really",
            "recklessly",
            "regularly",
            "reluctantly",
            "repeatedly",
            "rightfully",
            "roughly",
            "rudely",
            "sadly",
            "safely",
            "seldom",
            "selfishly",
            "seriously",
            "shakily",
            "sharply",
            "shrilly",
            "shyly",
            "silently",
            "sleepily",
            "slowly",
            "smoothly",
            "softly",
            "solemnly",
            "sometimes",
            "soon",
            "speedily",
            "stealthily",
            "sternly",
            "successfully",
            "suddenly",
            "suspiciously",
            "swiftly",
            "tenderly",
            "tensely",
            "thoughtfully",
            "tightly",
            "tomorrow",
            "too",
            "truthfully",
            "unexpectedly",
            "very",
            "victoriously",
            "violently",
            "vivaciously",
            "warmly",
            "weakly",
            "wearily",
            "well",
            "wildly",
            "yearly",
            "yesterday",
        };
        #endregion

        #region Adjectives
        private static readonly string[] adjective = new string[]
		{
            "able",
            "absurd",
            "acceptable",
            "adaptable",
            "adorable",
            "altruistic",
            "amazing",
            "ambitious",
            "amusing",
            "anglelic",
			"angry",
            "artful",
            "authentic",
            "awesome",
            "balanced",
			"beefy",
            "beneficial",
            "best ever",
            "big",
            "blissful",
            "blobby",
			"blue",
            "brainy",
            "brave",
            "breath-taking",
            "breezy",
            "bright",
            "brilliant",
            "bouyant",
			"brown",
            "calm",
            "captivating",
            "charming",
            "choice",
			"charismatic",
			"chartruese",
            "clever",
            "colorful",
            "comedic",
            "commendable",
            "compatible",
            "confident",
            "cool",
            "courageous",
            "credible",
            "cute",
            "daring",
            "decent",
            "decisive",
            "dependable",
            "devoted",
            "distinct",
			"doubtful",
            "dreamy",
            "earnest",
            "effecient",
            "elated",
            "elegant",
            "enchanting",
            "exact",
            "excellent",
            "exquisite",
			"fabulous",
            "faithful",
            "fantabulous",
            "fantastic",
            "fat",
            "favorablue",
            "feline",
            "flammable",
            "first-rate",
            "free",
            "fun",
            "funny",
            "generous",
            "gentle",
            "genuine",
            "glad",
            "gleeful",
            "gnarly",
            "good",    
            "grand",
            "grandiose",
            "great",
			"greenish",
            "groovy",
			"gullable",
            "handy",
            "happy",
            "hardy",
            "healthy",
            "helpful",
            "honorable",
            "hopeful",
            "hospitable",
            "humorous",
            "impressive",
            "influential",
            "ingenious",
            "inspired",
            "jolly",
            "joyful",
            "jubilant",
            "keen",
            "knowing",
			"languid",
			"lax",
			"lazy",
			"livid",
			"liliputian",
			"long",
            "lovely",
            "lucky",
            "ludicrous",
            "magnetic",
			"manly",
            "marvelous",
            "merciful",
			"miniscule",
            "modest",
			"morose",
			"muscular",
            "neat",
            "nifty",
            "noble",
			"obfuscated", 
			"obese",
			"obtuse",
			"orange",
            "open-minded",
            "outstanding",
            "peaceful",
            "perfect",
            "pixellated",
            "playful",
            "pleasant",
            "poetic",
            "precious",
            "premium",
			"pretty",
			"precocious",
			"preferred",
			"proactive",
			"provocative",
			"purple",
            "qualified",
            "quick",
            "radical",
            "radiant",
            "rare",
            "real",
            "reflective",
			"red",
			"ridiculous",
            "robust",
            "rocky",
            "savvy",
            "select",
            "sharp",
			"short",
			"sickly",
            "sincere",
			"skinny",
            "slick",
            "smart",
            "smashing",
            "smooth",
            "solid",
            "spirited",
			"stinky",
			"strong",
            "successful",
            "stupendous",
            "strong",
            "super",
            "swell",
			"tall",
            "tedious",
            "tender",
 			"terrible",
            "terrific",
			"thin",
            "tiny",
            "tidy",
            "tranquil",
            "tiring",
            "unique",
            "unreal",
            "upright",
            "valiant",
            "virtuous",
            "vivid",
			"vexed",
            "wavy",
            "wealthy",
            "whimsical",
			"whistful",
			"wide",
			"wimpy",
            "wise",
			"wonderful",
			"yellow",
            "youthful",
            "zesty"
		};
        #endregion

        private static readonly string[] noun = new string[]
		{
			"@NOUNANIMAL", 
			"@NOUNPERSON",
			"@NOUNPLACE", 
			"@NOUNTHING", 
		};

		private static readonly string[] nounAnimal = new string[]	 // Use a comma to denote a special plural form
		{
			"albatross,albatrosses",
			"alligator",
			"barnacle",
			"bear",
			"buffalo", 
			"bug",
            "cat",
			"cow", 
            "dog",
			"elephant",
			"fish,fish",
            "frog",
			"girbil",
			"goose,geese",
			"guinea pig",
			"hamster",
			"herring", 
			"koala",
			"lemming", 
            "lizard",
			"mole",
			"moose,moose",
			"mouse,mice",
			"muskrat",
			"naked mole rat", 
			"ostrich,ostriches",
			"ox,oxen",
			"pig",
			"rabbit",
			"rat",
			"bird",
            "squirrel",
			"tiger",
            "toad",
			"vulture",
			"wildebeast", 
		};

		private static readonly string[] nounThing = new string[]   // Use a comma to denote a special plural form
		{
			"clip",
			"dish,dishes",
			"flower",
			"lightbulb",
			"mug",
			"newspaper",
			"noise",
			"nose",
			"pencil",
			"wish,wishes",
			"software,software",
			"television",
			"keyboard",
			"slider",
			"button",
			"tree",
			"shrub",
			"foot",
			"plant",
			"ornament",
			"whistle",
			"flute",
			"scanner",
			"rocket",
			"satellite",
			"planet",
			"star",
			"square",
			"plumb bob",
			"hammer",
			"nail",
			"monkey wrench,monkey wrenches",
			"pick",
			"ruler",
			"podium",
			"bottle",
			"telephone",
			"teleprompter",
			"hat",
			"scarf",
			"pants,pants",
			"shorts,shorts",
			"sock",
			"horizon",
			"thought",
			"sunrise",
			"sunset",
			"airplane",
			"building",
			"window",
			"board",
			"shingle",
			"lamp",
			"sidewalk",
			"water",
			"pail",
			
		};

		private static readonly string[] nounPlace = new string[]   // Use a comma to denote a special plural form
		{
			"country,countries",
			"house",
			"island",
			"pad",
			"porch,porches",
			"yard",
			"stadium",
			"gazeebo",
			"hut",
			"mud pit",
			"peninsula",
			"hamlet",
			"villiage",
			"town",
			"city,cities",
			"hill",
			"plain",
			"canyon",
			"lake",
			"pond",
			"river",
			"school",
			"store",
			"discotech",
			"police station",
			"restaraunt",
			"garden",
			"melon patch",
			"street",
			"nursing home",
			"magic show",
			"concert",
			"cow pasture",
			"barn",
			"lab",
			"office",
			"place of employment,places of employment",
		};

		private static readonly string[] nounPerson = new string[]   // Use a comma to denote a special plural form
		{
			"kid",
			"thug",
			"singer",
			"plumber",
			"waiter",
			"diver",
			"flourist",
			"botonist",
			"chemist",
			"professor",
			"linguist",
			"juggler",
			"entymologist",
			"horseman,horsemen",
			"child, children",
			"baby,babies",
			"old man,old men",
			"ballarina",
			"extortionist",
			"contortionist",
			"fisherman,fishermen",
			"stewardess,stewardesses",
			"captain",
			"soldier",
			"dreamer",
			"pushy salesman, pushy salesmen",
			"slacker",
			"Canadian",
			"American",
			"hocky player",
			"tester",
			"developer",
			"manager",
		};

        private static readonly string[] nounSubjectivePersonalPronoun = new string[]   // Use a comma to denote a special plural form
		{
			"I,we",
			"you,you",
			"she,they",
			"he,they",
			"it,they",
            "someone,someone",
            "no one,no one",
            "everyone,everyone"
		};

        private static readonly string[] nounPossessivePersonalPronoun = new string[]   // Use a comma to denote a special plural form
		{
			"my,our",
			"your,your",
			"her,their",
			"his,their",
			"its,their",
            "someone's,someones'",
            "no one's,no ones'",
            "everyone's,everyones'"
		};

        private static readonly string[] nounObjectivePersonalPronoun = new string[]   // Use a comma to denote a special plural form
		{
			"me,us",
			"you,you",
			"her,them",
			"him,them",
			"it,them",
		};

		// TODO:  possessive(Mine, yours, his, theirs)	
		//			reciprocal (each other, one another)
		//			reflexive (himself, ourselves)
		//			interrogative (who, what, which
		// http://www.sinclair.edu/departments/dev/english/8pronouns.htm#Pronouns


		private static readonly string[] nameFullName = new string[]   
		{
			"@NAMEFIRSTNAME @NAMELASTNAME"
		};

        #region First Names
        private static readonly string[] nameFirstName = new string[]   
		{
            "Aaliyah",
            "Aaron",
            "Abigail",
            "Adam",
            "Adrian",
            "Albert",
            "Alex",
            "Alexa",
            "Alexander",
            "Alexandra",
            "Alexis",
            "Alfalfa",
            "Allison",
            "Alyssa",
            "Amanda",
            "Amelia",
            "Andrea",
            "Andrew",
            "Andy",
            "Angel",
            "Angelina",
            "Anna",
            "Anthony",
            "Antonio",
            "Ariana",
            "Arianna",
            "Ashley",
            "Audrey",
            "Austin",
            "Autumn",
            "Ava",
            "Avery",
            "Bambi",
            "Bashful",
            "Benjamin",
            "Bill",
            "Blake",
            "Bradley",
            "Brandon",
            "Brian",
            "Brian",
            "Brianna",
            "Brittney",
            "Brooke",
            "Brooklyn",
            "Bryan",
            "Buck",
            "Caleb",
            "Cameron",
            "Carlos",
            "Caroline",
            "Chadwick",
            "Charles",
            "Chase",
            "Chloe",
            "Christian",
            "Christopher",
            "Claire",
            "Cody",
            "Connor",
            "Constance",
            "Corey",
            "Curly",
            "Dakota",
            "Daniel",
            "David",
            "David",
            "Derek",
            "Destiny",
            "Devin",
            "Diana",
            "Dustin",
            "Dylan",
            "Ed",
            "Edward",
            "Elizabeth",
            "Ella",
            "Emily",
            "Emma",
            "Eric",
            "Eric",
            "Ethan",
            "Evan",
            "Evelyn",
            "Faith",
            "Gabriel",
            "Gabriella",
            "Gabrielle",
            "Garrett",
            "Grace",
            "Gracie",
            "Gregory",
            "Grumpy",
            "Hailey",
            "Haley",
            "Hannah",
            "Hans",
            "Harmony",
            "Hunter",
            "Ian",
            "Isabel",
            "Isabella",
            "Isabelle",
            "Jack",
            "Jacob",
            "Jada",
            "James",
            "Jane",
            "Jared",
            "Jasmine",
            "Jason",
            "Jason",
            "Jefferson",
            "Jeffrey",
            "Jenna",
            "Jennifer",
            "Jeremy",
            "Jesse",
            "Jessica",
            "Jesus",
            "Jill",
            "Jocelyn",
            "John",
            "John",
            "Jonathan",
            "Jordan",
            "Jordan",
            "Jorge",
            "Jose",
            "Joseph",
            "Joshua",
            "Juan",
            "Julia",
            "Justin",
            "Kaitlyn",
            "Katelyn",
            "Katherine",
            "Katie",
            "Kayla",
            "Kaylee",
            "Kenneth",
            "Kevin",
            "Kimberly",
            "Knut",
            "Kyle",
            "Kylie",
            "Larry",
            "Lauren",
            "Leah",
            "Leif",
            "Lillian",
            "Lily",
            "Logan",
            "Luis",
            "Luke",
            "Mackenzie",
            "Madeline",
            "Madison",
            "Makayla",
            "Marcus",
            "Maria",
            "Mariah",
            "Marissa",
            "Mark",
            "Mary",
            "Matthew",
            "Maya",
            "Megan",
            "Melanie",
            "Mia",
            "Michael",
            "Michelle",
            "Mickey",
            "Miguel",
            "Mitchell",
            "Moe",
            "Molly",
            "MoonUnit",
            "Morgan",
            "Mork",
            "Mya",
            "Naresh",
            "Natalie",
            "Nate",
            "Nathan",
            "Nathaniel",
            "Nevaeh",
            "Nicholas",
            "Nicole",
            "Noah",
            "Olivia",
            "Paige",
            "Patrick",
            "Paul",
            "PeeWee",
            "Peter",
            "Phil",
            "Rachel",
            "Rebecca",
            "Richard",
            "Riley",
            "Robert",
            "Rodney",
            "Ryan",
            "Samantha",
            "Samuel",
            "Sara",
            "Sarah",
            "Savannah",
            "Scott",
            "Sean",
            "Seth",
            "Shane",
            "Shawn",
            "Shemp",
            "Sleepy",
            "Sofia",
            "Sophia",
            "Spanky",
            "Spencer",
            "Sri",
            "Stephanie",
            "Stephen",
            "Steve",
            "Steven",
            "Sydney",
            "Taylor",
            "Taylor",
            "Theodore", 
            "Thomas",
            "Timothy",
            "Travis",
            "Trevor",
            "Trinity",
            "Tyler",
            "Vanessa",
            "Victor",
            "Victoria",
            "Vincent",
            "William",
            "William",
            "Yule",
            "Yunbo",
            "Zachary",
            "Zoe",
        };

        #endregion
        #region Last Names
        private static readonly string[] nameLastName = new string[]   
		{
            "Adams",
            "Alexander",
            "Allen",
            "Anderson",
            "Andrews",
            "Armstrong",
            "Arnold",
            "Arumagum",
            "Austin",
            "Bailey",
            "Baker",
            "Baker",
            "Barber",
            "Barnes",
            "Baum",
            "Bell",
            "Bennett",
            "Berry",
            "Black",
            "Boyd",
            "Bradley",
            "Brenner",
            "Brian",
            "Brooks",
            "Brooks",
            "Brown",
            "Bryant",
            "Burns",
            "Butler",
            "Camp",
            "Campbell",
            "Carpenter",
            "Carroll",
            "Carter",
            "Chavez",
            "Clark",
            "Clinton",
            "Cole",
            "Coleman",
            "Collins",
            "Cook",
            "Cooper",
            "Cox",
            "Crawford",
            "Cruz",
            "Cunningham",
            "Daniels",
            "Davis",
            "Deng",
            "Diaz",
            "Dixon",
            "Duncan",
            "Dunn",
            "Edwards",
            "Einstein",
            "Elliott",
            "Ellis",
            "Evans",
            "Fenning",
            "Ferguson",
            "Fisher",
            "Flores",
            "Ford",
            "Foster",
            "Fox",
            "Franklin",
            "Freeman",
            "Garcia",
            "Gardner",
            "Gaylord",
            "Gibson",
            "Gomez",
            "Gonzales",
            "Gonzalez",
            "Gordon",
            "Graham",
            "Grant",
            "Gray",
            "Green",
            "Green",
            "Greene",
            "Griffin",
            "Hall",
            "Hamilton",
            "Harper",
            "Harris",
            "Harrison",
            "Hart",
            "Hawking",
            "Hawkins",
            "Hayes",
            "Henderson",
            "Henry",
            "Hernandez",
            "Hicks",
            "Hill",
            "Holmes",
            "Howard",
            "Hudson",
            "Hughes",
            "Hunt",
            "Hunter",
            "Jackson",
            "Jackson",
            "James",
            "Jefferson",
            "Jenkins",
            "Johnson",
            "Jones",
            "Jordan",
            "Jordan",
            "Jorgensen",
            "Katz",
            "Keith",
            "Kelley",
            "Kelly",
            "Kennedy",
            "King",
            "Knight",
            "Kohler",
            "Lane",
            "Lawrence",
            "Lawson",
            "Lee",
            "Lewis",
            "Long",
            "Lopez",
            "Lopez",
            "MacDonald",
            "Mackeral",
            "Major",
            "Marshall",
            "Martin",
            "Martinez",
            "Mason",
            "Matthews",
            "Mcdonald",
            "Miller",
            "Mills",
            "Mitchell",
            "Moore",
            "Morales",
            "Morgan",
            "Morris",
            "Mouse",
            "Murphy",
            "Murray",
            "Myers",
            "Nelson",
            "Nichols",
            "Olson",
            "Ortiz",
            "Owens",
            "Palmer",
            "Parker",
            "Patterson",
            "Payne",
            "Perez",
            "Perkins",
            "Perry",
            "Peters",
            "Peterson",
            "Phillips",
            "Pierce",
            "Pitts",
            "Porter",
            "Powell",
            "Price",
            "Ramirez",
            "Ramos",
            "Ray",
            "Reddenbokker",
            "Reed",
            "Reyes",
            "Reynolds",
            "Rhodes",
            "Rice",
            "Richardson",
            "Riley",
            "Rivera",
            "Roberts",
            "Robertson",
            "Robinson",
            "Rodriguez",
            "Rogers",
            "Roosevelt",
            "Rose",
            "Ross",
            "Ruiz",
            "Russell",
            "Sanchez",
            "Sanders",
            "Scott",
            "Shaw",
            "Simmons",
            "Simpson",
            "Sims",
            "Smith",
            "Smith",
            "Smooch",
            "Snyder",
            "Spencer",
            "Stephens",
            "Stevens",
            "Stewart",
            "Stone",
            "Sullivan",
            "Taylor",
            "Thomas",
            "Thompson",
            "Torres",
            "Tucker",
            "Turner",
            "Wagner",
            "Walker",
            "Wallace",
            "Ward",
            "Warren",
            "Washington",
            "Watkins",
            "Watson",
            "Weaver",
            "Webb",
            "Wells",
            "West",
            "White",
            "Williams",
            "Willis",
            "Wilson",
            "Woo",
            "Wood",
            "Woods",
            "Wright",
            "Young",
            "Zigler",
		};
        #endregion

		private static readonly string[] namePlace = new string[]   // Use a comma to denote a special plural form
		{
			"Albuquerque",
			"Detroit",
			"El Paso",
			"Italy",
			"Kentuky",
			"Walla Walla",
			"Seattle",
			"Redmond",
			"Everett",
			"The South Pole",
			"Paradise City",
			"Las Vegas",
			"Pioria",
			"Turkey",
			"Zimbabwe",
			"Tulsa",
			"Boston",
			"Wimbleton",
			"Alaska",
			"Hawaii",
			"Madagascar",
			"Moses Lake",
			"Boise",
			"Chicago",
            "Moscow",
            "Karachi",
            "Tokyo",
            "New Mexico",
            "Ohio",
            "France",
            "Spain",
            "Japan",
            "Mexico",
            "Texas",
            "Dallas",
            "Seattle",
            "Washington",
            "Canada",
            "Mt. Everest",
            "Lake Michigan",
            "Brasil",
            "India",
            "Australia",
            "Iceland",
            "Germany",
            "Russia",
            "Siberia",
            "Mars",
            "Topeka",
            "Chattanuga",
            "Norway",
            "Egypt",
            "Deluth",
            "Buffalo",
            "Erie",
		};

		private static readonly string[] verbIntransitive = new string[] // Use a comma to separate past,present,present participle forms
		{
			"crumbled,crumble,crumbling",
			"blinked,blink,blinking",
			"fell,fall,falling",
			"finished,finish,finishing",
			"gesticulated,gesticulate,gesticulating",
			"hesitated,hesitate,hesitating",
			"jumped,jump,jumping",
			"kneaded,knead,kneading",
			"lunged,lunge,lunging",
			"painted,paint,painting", 
			"prepared,prepare,preparing",
			"ran,run,running",
			"read,read,reading",
			"sat,sit,sitting",
			"sang,sing,singing",
			"slid,slide,sliding",
			"studied,study,studying",
			"smoked,smoke,smoking",
			"sniffed,sniff,sniffing", 
			"surprised,surprise,surprising",
			"swam,swim,swimming",
			"taught,teach,teaching",
			"understood,understand,understanding",
			"walked,walk,walking",
			"wished,wish,wishing",
			"worried,worry,worrying",
			"yelled,yell,yelling",
		};

		private static readonly string[] verbTransitive = new string[] 
		{
			"ate,eat,eating",
			"built,build,building",
			"equipped,equip,equipping",
			"finished,finish,finishing",
			"fetched,fetch,fetching",
			"flicked,flick,flicking",
			"flipped,flip,flipping",
			"flogged,flog,flogging",
			"hugged,hug,hugging",
			"ingested,ingest,ingesting",
			"kneaded,knead,kneading",
			"lifted,lift,lifting",
			"liked,like,liking",
			"lowered,lower,lowering",
			"mugged,mug,mugging",
			"painted,paint,painting",
			"picked,pick,picking",
			"plowed,plow,plowing",
			"prepared,prepare,preparing",
			"pulled,pull,pulling",
			"read,read,reading",
			"scratched,scratch,scratching",
			"slid,slide,sliding", 
			"studied,study,studying",
			"sung,sing,singing",
			"surprised,surprise,surprising",
			"smashed,smash,smashing",
			"smoked,smoke,smoking",
			"smoothed,smooth,smoothing",
			"taught,teach,teaching",
			"understood,understand,understanding",
		};

		private static readonly string[] topLevelDomain = new string[]
		{
			"com","net","org","gov","us","biz",
		};

		#endregion

		#region initialization

		// *********************************************************************
		/// <summary>
		/// Constructor
		/// </summary>
		// *********************************************************************
		public MadLib()
		{
			Clear();
		}

		// *************************************************************************
		/// <summary>
		/// Clear out the state data
		/// </summary>
		// *************************************************************************
		public void Clear()
		{
 			for(int i = 0; i < identities.Length; i++) 
			{
				identities[i] = new Identity();
			}

			randomName = "";
            currentIdentity = 255;
		}

		#endregion

		#region parsing code

		// *********************************************************************
		/// <summary>
		/// Helper function to extract the single part of a comma separated word string
		/// </summary>
		/// <param name="wordString"></param>
		/// <returns></returns>
		// *********************************************************************
		private string SinglePart(string wordString)
		{
			string[] parts = wordString.Split(',');
			return parts[0];
		}

		// *********************************************************************
		/// <summary>
		/// helper function for generating a random string from an array.
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		// *********************************************************************
		private string RandomString(string[] stringArray)
		{
			int i = random.Next(stringArray.Length);

			return stringArray[i];
		}

		// *********************************************************************
		/// <summary>
		/// If the part is a string directive, it is processed and replaced
		/// </summary>
		/// <param name="part"></param>
		/// <returns></returns>
		// *********************************************************************
		private string ProcessPart(string part)
		{
			Identity myIdentity = null;
			bool isNoun = false;  
			bool isVerb = false;
			bool isBe = false;
            bool statedInfinitive = false;
            int statedId = currentIdentity--;
			int statedTense = Rand.Next(3);
            int statedPlurality = Rand.Next(2);
            bool statedProgressive = false;

            Match partMatch = Regex.Match(part, @"^@(\w+)(\W\d*)*$");

            if (!partMatch.Success) return part;

            foreach (Capture capture in partMatch.Groups[2].Captures)
            {
                char code = capture.Value[0];
                string valueString = capture.Value.Substring(1);
                int number = valueString.Length > 0 ? int.Parse(valueString) : -1;

                switch (code)
                {
                    case '.': statedId = number % 256; break;
                    case '^': statedTense = number % 3; break;
                    case '*': statedPlurality = number % 2; break;
                    case '~': statedInfinitive = true; break;
                    case '!': statedProgressive = true; break;
                    default: return "[[BAD CODE (" + code + ") in " + part + "]]"; 
                }
            }

            myIdentity = identities[statedId]; 

            if (!myIdentity.IsReserved)
            {
                myIdentity.IsPlural = statedPlurality == 1;
                myIdentity.IsReserved = true;
            }
            
            myIdentity.IsInfinitive = statedInfinitive;
            myIdentity.Tense = statedTense;
            myIdentity.IsProgressive = statedProgressive;

            string wordType = partMatch.Groups[1].Value.ToUpper();

			// Nouns have special processing later
            if (wordType.IndexOf("NOUN") == 0) isNoun = true;
            if (wordType.IndexOf("VERB") == 0) isVerb = true;
            if (wordType.IndexOf("BE") == 0) isBe = true;

            switch (wordType)
			{
				case "PUNCTUATION": 
					part = RandomString(punctuation);
					break;

                case "ADJECTIVE":
                    part = RandomString(adjective);
                    break;
                case "ADVERB":
                    part = RandomString(adverb);
                    break;
                case "ARTICLE": 
					if(myIdentity.IsPlural) part = RandomString(pluralArticle);
					else part = RandomString(singleArticle);
					break;
				case "BE":
					break;
                case "INTERROGATIVE":
                    part = RandomString(interrogative);
                    break;
                case "MODALVERB":
					part = RandomString(modalVerb);
					break;

				case "NAMEFULLNAME":
					part = RandomString(nameFullName);
					break;
				case "NAMEFIRSTNAME":
					part = RandomString(nameFirstName);
					break;
				case "NAMELASTNAME":
					part = RandomString(nameLastName);
					break;
				case "NAMEPLACE":
					part = RandomString(namePlace);
					break;

				case "NOUN":
					part = RandomString(noun);
					break;
				case "NOUNANIMAL":
					part = RandomString(nounAnimal);
					break;
				case "NOUNPERSON":
					part = RandomString(nounPerson);
					break;
				case "NOUNPLACE":
					part = RandomString(nounPlace);
					break;
				case "NOUNOBJECTIVEPERSONALPRONOUN":
					part = RandomString(nounObjectivePersonalPronoun);
					break;
                case "NOUNSUBJECTIVEPERSONALPRONOUN":
                    part = RandomString(nounSubjectivePersonalPronoun);
                    break;
                case "NOUNPOSSESSIVEPERSONALPRONOUN":
                    part = RandomString(nounPossessivePersonalPronoun);
                    break;
                case "NOUNTHING":
					part = RandomString(nounThing);
					break;

				case "PREPOSITION":
					part = RandomString(preposition);
					break;

				case "VERBTRANSITIVE":
					part = RandomString(verbTransitive);
					break;
				case "VERBINTRANSITIVE":
					part = RandomString(verbIntransitive);
					break;
				case "TOPLEVELDOMAIN":
					part = RandomString(topLevelDomain);
					break;
				default:
					return "*** ERROR(" + part + ") ***";
			}

			// post process finalized words
			if(part.IndexOf('@') == -1) 
			{
				// Handle plural nouns
				if(isNoun) 
				{
					if(myIdentity.IsPlural)
					{
						if(part.IndexOf(',') > -1) part = part.Split(',')[1];
						else part = part + "s";
					}
					else part = part.Split(',')[0];
				}

				// Handle verb tense
				if(isVerb)
				{
                    string[] tenseParts = part.Split(',');
                    part = myIdentity.Tense == 0 ? tenseParts[0] : tenseParts[1];
                    string personalPronoun = myIdentity.Word("NOUNSUBJECTIVEPERSONALPRONOUN");

                    if (myIdentity.IsProgressive)
                    {
                        part = "@BE " + tenseParts[2];
                    }
					else if(myIdentity.Tense == 1)
					{
                        // Present participle
                        if (!myIdentity.IsPlural)
                        {
						    if(!myIdentity.IsInfinitive && personalPronoun != "you" && personalPronoun != "I" && personalPronoun != "we")
							    part += "s";
                        }
                        else if (personalPronoun.EndsWith("one")) part = part + "s";

                        if (personalPronoun != "") part = "@BE " + part;
					}
				}

				myIdentity.Attach(wordType,part);
			}

            // Handle plural and singular forms of be
            if (isBe)
            {
                string personalPronoun = myIdentity.Word("NOUNSUBJECTIVEPERSONALPRONOUN");
                switch (myIdentity.Tense)
                {
                    case 0:
                        part = myIdentity.IsPlural ? "were" : "was";
                        if (Regex.IsMatch(personalPronoun, "(you|we|they)")) part = "were";
                        else if (Regex.IsMatch(personalPronoun, "(I)")) part = "was";
                        else if (personalPronoun != "") part = "was";

                        if (Rand.Next(2) == 0) part = "@MODALVERB have been";
                        break;
                    case 1:
                        part = myIdentity.IsPlural ? "are" : "is";
                        if (Regex.IsMatch(personalPronoun, "(you|we|they)")) part = "are";
                        else if (Regex.IsMatch(personalPronoun, "(I)")) part = "am";
                        else if (personalPronoun != "") part = "is";
                        break;
                    case 2:
                        part = "@MODALVERB be";
                        break;
                }

                if (myIdentity.IsPlural && part.IndexOf(',') > -1) part = part.Split(',')[1];
                else part = part.Split(',')[0];
            }

			// Preserve special modifiers
			if(part.IndexOf('@') > -1) 
			{
				// first find where to insert the modifiers
				int commandIndex = part.IndexOf('@');
				int insertIndex = part.IndexOf(' ',commandIndex);
				if(insertIndex == -1) insertIndex = part.Length;

				// build the modifier string
				string modifierString = "";
                if (myIdentity.IsInfinitive) modifierString += "~";
                if (myIdentity.IsProgressive) modifierString += "!";
                modifierString += "*" + (myIdentity.IsPlural ? 1 : 0);
                modifierString += "^" + myIdentity.Tense;
				if (statedId > -1) modifierString += "." + statedId;

				// return the string with the modifiers added
				part = part.Insert(insertIndex,modifierString);
			}


			return part;
		}

		// *********************************************************************
		/// <summary>
		///	Recursive function for parsing strings with speech directives in them
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		// *********************************************************************
		private string ProcessString(string template, int depth)
		{
            if(depth > 10) return template;

			string newString = "";
			string separator = "";
			string[] parts = template.Split(' ');

			
			foreach (string part in parts)
			{
				newString  += separator + ProcessPart(part);
				separator = " ";
			}

			if(newString.IndexOf('@') > -1) return ProcessString(newString, depth + 1);

			return newString;
		}

		// *************************************************************************
		/// <summary>
		/// fix punctuation and certain word usage
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		// *************************************************************************
		public string PostProcessString(string fixme)
		{
			// Fix punctuation
			string str = fixme.Replace(" ,",",");
			str = str.Replace(" .",".");
			str = str.Replace(" !","!");
			str = str.Replace(" ?","?");
			str = str.Replace(" ;",";");

			// Fix article "a" usage
			str = str.Replace(" a a"," an a");
			str = str.Replace(" a e"," an e");
			str = str.Replace(" a i"," an i");
			str = str.Replace(" a o"," an o");
			str = str.Replace(" a u"," an u");

			// Capitalize first letter
			str = str[0].ToString().ToUpper() + str.Remove(0,1);

			return str;
		}

		#endregion

		#region GetString functions

		// *********************************************************************
		/// <summary>
		///	 Generate a completely random string
		/// </summary>
		/// <param name="sizeInChars"></param>
		/// <returns></returns>
		// *********************************************************************
		public string GetString()
		{
			string outString = RandomString(MadlibSentance);

			Clear();

			// return processed template
			return PostProcessString(ProcessString(outString, 0));
		}

		// *************************************************************************
		/// <summary>
		/// Returns a random string based on the type you pass in
		/// </summary>
		/// <param name="madlibType"></param>
		/// <returns></returns>
		// *************************************************************************
		public string GetString(string[] madlibType)
		{
			string outString = RandomString(madlibType);

			Clear();

			// return processed template
			return PostProcessString(ProcessString(outString, 0));
		}

		// *************************************************************************
		/// <summary>
		/// returns a random string based on the specific template passed in
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		// *************************************************************************
		public string GetString(string template)
		{
			Clear();

			// return processed template
			return PostProcessString(ProcessString(template, 0));
		}

		// *************************************************************************
		/// <summary>
		/// Returns a Random Email Address based on the previously gotten name
		/// </summary>
		/// <returns></returns>
		// *************************************************************************
		public string GetEmail()
		{
			if(randomName == "") GetName();
			string emailAddress = randomName + "@" + ProcessString("@NOUN @NOUN @TOPLEVELDOMAIN", 0);
			emailAddress = emailAddress.ToLower().Replace(" ",".");
			return emailAddress;

		}

		// *************************************************************************
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		// *************************************************************************
		public string GetName()
		{
			randomName = ProcessString("@NAMEFULLNAME", 0);
			return randomName;
		}

		#endregion


	}
}
