internal class Program
{
    private static void Main(string[] args)
    {
        // Gather data
        if (!TakeInput(args, out string startingGene, out string targetGene, out string[] possibleGenes))
            Console.WriteLine("Invalid inputs.");
        // Calculate
        List<char> lettersToMutate = FindNeededMutations(startingGene, targetGene);
        List<string> mutationPath = ConstructMutationPath(lettersToMutate, startingGene, possibleGenes.ToList());
        if (mutationPath == null) // Check if a solution was found and if not, output a -1
        {
            Console.WriteLine(-1);
            return;
        }
        // Output data
        PushOutput(mutationPath);
    }

    #region Input
    /// <summary>
    /// Gathers all data needed to perform the genetic calculations
    /// </summary>
    /// <param name="args">The command line arguments of this program</param>
    /// <param name="startingGene">An out parameter for the starting gene</param>
    /// <param name="targetGene">An out parameter for the target gene</param>
    /// <param name="possibleGenes">An out parameter for the possible genes</param>
    /// <returns>True if all the data is valid, otherwise false</returns>
    static bool TakeInput(string[] args, out string startingGene, out string targetGene, out string[] possibleGenes)
    {
        // The only letters that can make a valid gene
        char[] validLetters =
        {
            'A',
            'C',
            'G',
            'T'
        };

        startingGene = "";
        targetGene = "";
        possibleGenes = Array.Empty<string>();

        // Check if the specified gene file exists
        if (args.Length == 0 || !File.Exists(args[0]))
            return false;

        // Get the starting and target gene from the console input
        if (!GetGeneFromConsole(validLetters, "Starting gene: ", out startingGene) || !GetGeneFromConsole(validLetters, "Target gene: ", out targetGene))
            return false;

        // Get all the gene possibilities from the specified file
        possibleGenes = LoadGenesFromFile(args[0], validLetters);

        return true;
    }

    /// <summary>
    /// Gets a single gene from the console, checks if it's valid, and returns it
    /// </summary>
    /// <param name="validLetters">Letters valid genes can consist of</param>
    /// <param name="prompt">A prompt to be written into the console to ask for input</param>
    /// <param name="gene">An out parameter to plug in a gene variable to</param>
    /// <returns>True if the user's gene is valid, otherwise false</returns>
    static bool GetGeneFromConsole(char[] validLetters, string prompt, out string gene)
    {
        Console.Write(prompt);

        gene = Console.ReadLine();

        // Check if the gene is the correct length
        if (gene == null || gene.Length != 8)
            return false;

        // Check if all the letters of the gene are valid
        foreach (char c in gene.ToUpper())
        {
            if (!validLetters.Contains(c))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Loads all genes written in a single file
    /// </summary>
    /// <param name="path">A path leading to the gene file</param>
    /// <param name="validLetters">An array of valid letters a gene can consist of</param>
    /// <returns>The loaded genes</returns>
    static string[] LoadGenesFromFile(string path, char[] validLetters)
    {
        List<string> genes = new();
        StreamReader sr = new(path);

        while (!sr.EndOfStream)
        {
            string gene = "";
            while (gene.Length < 8 && !sr.EndOfStream)
            {
                char input = (char)sr.Read();
                // Add the last read character to the gene code if it belongs to the valid characters
                // We have to cast it to string, uppercase it and cast to char again to allow lowercase inputs as well
                if (validLetters.Contains(input.ToString().ToUpper().ToCharArray()[0]))
                    gene += input;
            }

            genes.Add(gene);
        }

        return genes.ToArray();
    }
    #endregion

    #region Calculations
    /// <summary>
    /// Finds all mutations we need to perform on the starting gene to get to the target gene
    /// </summary>
    /// <param name="startingGene">The starting gene</param>
    /// <param name="targetGene">The target gene</param>
    /// <returns>A list of all the mutations we have to do</returns>
    static List<char> FindNeededMutations(string startingGene, string targetGene)
    {
        List<char> lettersToMutate = new();

        for (int i = 0; i < startingGene.Length; i++)
        {
            // The gene letters at this position match - no changes to the genetic code have to be made
            if (startingGene[i] == targetGene[i])
                continue;

            lettersToMutate.Add(targetGene[i]);
        }

        return lettersToMutate;
    }

    /// <summary>
    /// Constructs a "mutation path". 
    /// A mutation path is a list of genes we mutate one by one to get to the target gene
    /// </summary>
    /// <param name="lettersToMutate">The list of changes we have to make</param>
    /// <param name="startingGene">The starting gene</param>
    /// <param name="possibleGenes">All the genes we can use</param>
    /// <returns>A list of genes we have during the process of mutation</returns>
    static List<string> ConstructMutationPath(List<char> lettersToMutate, string startingGene, List<string> possibleGenes)
    {
        List<string> mutationPath = new()
        {
            startingGene
        };
        string currentGene = startingGene;

        foreach (char gene in lettersToMutate)
        {
            // Stores if a valid next "step" of the path was found
            bool stepFound = false;

            // Find a matching gene that does this exact mutation we need
            for (int i = 0; i < possibleGenes.Count; i++)
            {
                // Check if the current gene and the mutation option we are checking differ
                // by at most one letter and if the different letter is the one we are looking for
                if (GetDifference(currentGene, possibleGenes[i], out char diff) && diff == gene)
                {
                    currentGene = possibleGenes[i];
                    mutationPath.Add(possibleGenes[i]);
                    possibleGenes.RemoveAt(i);
                    stepFound = true;
                }
            }

            if (!stepFound)
                return null;
        }

        return mutationPath;
    }

    /// <summary>
    /// Checks the difference between two genes.
    /// Outputs true if there is only one different character, otherwise false.
    /// Also outputs the character making the difference between the two genes (taken from the target)
    /// </summary>
    /// <param name="current">The current gene</param>
    /// <param name="target">The target gene</param>
    /// <param name="difference">An out parameter for the different letter</param>
    /// <returns></returns>
    static bool GetDifference(string current, string target, out char difference)
    {
        int diffCount = 0;
        difference = ' ';

        // Go through both of the genes
        for (int i = 0; i < current.Length; i++)
        {
            // If the i-th character in both of the genes do not match, calculate the difference
            if (current[i] != target[i])
            {
                diffCount++;
                difference = target[i];
            }
        }

        // Output true if and only if there was one difference found
        return diffCount == 1;
    }
    #endregion

    #region Output
    /// <summary>
    /// Gives the user some visual feedback in the form of calculation results
    /// </summary>
    /// <param name="mutationPath">A mutation path to be written into the console</param>
    static void PushOutput(List<string> mutationPath)
    {
        Console.WriteLine("Mutation count: " + mutationPath.Count);
        foreach (string gene in mutationPath)
            Console.WriteLine(gene);
    }
    #endregion
}