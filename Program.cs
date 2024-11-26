using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create an HttpClient instance
        using (var httpClient = new HttpClient())
        {
            string site = "https://bymykel.github.io/CSGO-API/api/en/collectibles/operation/coins.json";
            // List of URLs to download
            int pages = 1;
            string[] urls = new string[pages];
            for (int s = 0; s < pages; s++) urls[s] += $"{site}";//?page={s+1}";


            string outputFilePath = "outCoins.txt";
            string modifiedContents = "";
            modifiedContents = modifiedContents.Insert(0, "$$$$$Autograph" + Environment.NewLine + "$$$$$Teams" + Environment.NewLine);
            int i = 0;
            string[] rarity = new string[7000];
            string[] major = new string[7000];
            string[] type = new string[7000];
            string[] capsule = new string[7000];
            bool[] autograph = new bool[7000];
            foreach (string url in urls)
            {
                try
                {
                    // Make an HTTP GET request to the URL
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the HTML content
                        string htmlContent = await response.Content.ReadAsStringAsync();

                        string pattern = @"""id"": ""collectible[\s\S]*?""name"": ""([\s\S]*?)""";
                        MatchCollection matches = Regex.Matches(htmlContent, pattern, RegexOptions.IgnoreCase);

                        foreach (Match match in matches)
                        {
                            //Initialize
                            string name = match.Groups[1].Value.Trim();
                            string[] words = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            int place = 0;
                            foreach(string word in words)
                            {
                                if(word=="Operation")
                                {
                                    place++;
                                    break;
                                }
                                place++;
                            }
                            
                            rarity[i] = "No rarity";
                            type[i] = words[place];
                            capsule[i] = match.Groups[5].Value.Trim();
                            
                            //if (type[i] == "") type[i] = "Standard";
                            if (capsule[i] == "") capsule[i] = "No Capsule";
                            //if (major[i] == "") 
                            major[i] = "No major";
                            //Check AUTOGRAPH
                            if (capsule[i].Contains("Autograph"))
                            {
                                autograph[i] = true;
                            }
                            else autograph[i] = false;
                            //Cycle to find matches
                            bool sameCapsule = false;
                            bool sameRarity = false;
                            bool sameType = false;
                            for (int a = 0; a < i; a++)
                            {
                                if (type[a] == type[i] && rarity[a] == rarity[i] && capsule[a] == capsule[i])
                                    sameType = true;
                                if (rarity[a] == rarity[i] && capsule[a] == capsule[i])
                                    sameRarity = true;
                                if (capsule[a] == capsule[i])
                                    sameCapsule = true;
                            }

                            //Insert index
                            Console.WriteLine("PASS0");
                            string autographs = modifiedContents.Substring(0, modifiedContents.IndexOf("Teams"));

                            string teams = modifiedContents.Substring(modifiedContents.IndexOf("Teams"), modifiedContents.Length - modifiedContents.IndexOf("Teams") - 1);
                            //Console.WriteLine("PASS");
                            //Console.WriteLine(autograph[i]);
                            //Insert MAJOR
                            int insertionGraph;
                            if (autograph[i])
                            {
                                insertionGraph = modifiedContents.IndexOf("Autograph") + 9;
                            }
                            else
                            {
                                insertionGraph = modifiedContents.IndexOf("Teams") + 5;
                            }
                            if (!autographs.Contains(major[i]) && autograph[i] || !teams.Contains(major[i]) && !autograph[i])
                            {
                                modifiedContents = modifiedContents.Insert(insertionGraph, Environment.NewLine + ">>>>>>>>>" + major[i]);
                            }
                            //Insert Capsule
                            autographs = modifiedContents.Substring(0, modifiedContents.IndexOf("Teams"));
                            teams = modifiedContents.Substring(modifiedContents.IndexOf("Teams"), modifiedContents.Length - modifiedContents.IndexOf("Teams") - 1);

                            int insertionIndex;
                            if (autograph[i])
                            {
                                insertionIndex = autographs.IndexOf(major[i]) + major[i].Length;
                            }
                            else
                            {
                                insertionIndex = teams.IndexOf(major[i]) + modifiedContents.IndexOf("Teams") + major[i].Length;
                            }

                            //Check Capsule Contains
                            if (autographs.Contains(capsule[i]) && autograph[i] || teams.Contains(capsule[i]) && !autograph[i])
                            {
                                if (!sameCapsule) modifiedContents = modifiedContents.Insert(insertionIndex, Environment.NewLine + Environment.NewLine + "!" + capsule[i]);

                            }
                            if (!autographs.Contains(capsule[i]) && autograph[i] || !teams.Contains(capsule[i]) && !autograph[i])
                            {
                                //Console.WriteLine($"HEY {i}");
                                modifiedContents = modifiedContents.Insert(insertionIndex, Environment.NewLine + Environment.NewLine + "!" + capsule[i]);
                            }
                            //Insert RARITY
                            autographs = modifiedContents.Substring(0, modifiedContents.IndexOf("Teams"));
                            teams = modifiedContents.Substring(modifiedContents.IndexOf("Teams"), modifiedContents.Length - modifiedContents.IndexOf("Teams") - 1);
                            //Console.WriteLine("PASS2");
                            int insertionIndexCapsule;
                            if (autograph[i])
                            {
                                insertionIndexCapsule = autographs.IndexOf(capsule[i]) + capsule[i].Length;
                            }
                            else
                            {
                                insertionIndexCapsule = teams.IndexOf(capsule[i], insertionIndex - modifiedContents.IndexOf("Teams")) + modifiedContents.IndexOf("Teams") + capsule[i].Length;
                            }
                            //Console.WriteLine(insertionIndex);
                            //Check Rarity Contains
                            if (autographs.Contains(rarity[i]) && autograph[i] || teams.Contains(rarity[i]) && !autograph[i])
                            {
                                if (!sameRarity) modifiedContents = modifiedContents.Insert(insertionIndexCapsule, Environment.NewLine + ">" + rarity[i] + Environment.NewLine);
                            }

                            if (!autographs.Contains(rarity[i]) && autograph[i] || !teams.Contains(rarity[i]) && !autograph[i])
                            {
                                Console.WriteLine($"HEY {i}");
                                modifiedContents = modifiedContents.Insert(insertionIndexCapsule, Environment.NewLine + ">" + rarity[i] + Environment.NewLine);
                            }
                            //Insert TYPE

                            autographs = modifiedContents.Substring(0, modifiedContents.IndexOf("Teams"));
                            teams = modifiedContents.Substring(modifiedContents.IndexOf("Teams"), modifiedContents.Length - modifiedContents.IndexOf("Teams") - 1);

                            int insertionIndex2;
                            if (autograph[i])
                            {
                                insertionIndex2 = autographs.IndexOf(rarity[i], insertionIndexCapsule) + rarity[i].Length;
                            }
                            else
                            {
                                insertionIndex2 = teams.IndexOf(rarity[i], insertionIndexCapsule - modifiedContents.IndexOf("Teams")) + rarity[i].Length + modifiedContents.IndexOf("Teams");
                            }
                            //Check Type Contains
                            if (type[i] != "")
                            {
                                if (autographs.Contains(type[i]) && autograph[i] || teams.Contains(type[i]) && !autograph[i])
                                {
                                    if (!sameType) modifiedContents = modifiedContents.Insert(insertionIndex2, Environment.NewLine + Environment.NewLine + "#" + type[i]);

                                }

                                if (!autographs.Contains(type[i]) && autograph[i] || !teams.Contains(type[i]) && !autograph[i])
                                {

                                    modifiedContents = modifiedContents.Insert(insertionIndex2, Environment.NewLine + Environment.NewLine + "#" + type[i]);
                                }
                            }

                            //Insert NAME
                            //string type2 = type[i] + Environment.NewLine;
                            autographs = modifiedContents.Substring(0, modifiedContents.IndexOf("Teams"));
                            teams = modifiedContents.Substring(modifiedContents.IndexOf("Teams"), modifiedContents.Length - modifiedContents.IndexOf("Teams") - 1);

                            int insertionIndex3;
                            if (autograph[i])
                            {
                                insertionIndex3 = autographs.IndexOf(type[i] + Environment.NewLine, insertionIndex2) + type[i].Length;
                            }
                            else
                            {
                                insertionIndex3 = teams.IndexOf(type[i] + Environment.NewLine, insertionIndex2 - modifiedContents.IndexOf("Teams")) + type[i].Length + modifiedContents.IndexOf("Teams");
                            }
                            Console.WriteLine(insertionIndex3);
                            /*for (int a = 0; a < i; a++) if (type[a] == type[i])
                                {
                                    insertionIndex3 = modifiedContents.IndexOf(type[i]+"\n") + type[i].Length;

                                }*/

                            //Console.WriteLine($"{i} " + insertionIndex + " " + insertionIndex2);
                            //if (insertionIndex3 != -1)
                            {
                                modifiedContents = modifiedContents.Insert(insertionIndex3, Environment.NewLine + match.Groups[1].Value.Trim());//+ " " + match.Groups[3].Value.Trim()
                            }
                            Console.WriteLine($"{i} " + rarity[i] + ">" + major[i] + " " + " " + type[i] + ">" + match.Groups[1].Value.Trim() + $"{insertionIndex}" + " " + $"{insertionIndex3}");

                            i++;

                        }

                        Console.WriteLine($"Sorted: {url}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to sort: {url}");
                    }
                    i++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            File.WriteAllText(outputFilePath, modifiedContents);
            StringBuilder escapedString = new StringBuilder();

            /*foreach (char c in modifiedContents)
            {
                if (char.IsControl(c))
                {
                    escapedString.Append($"\\u{(int)c:X4}");
                }
                else
                {
                    escapedString.Append(c);
                }
            }*/


            // Convert the StringBuilder to a string
            string result = escapedString.ToString();

            Console.WriteLine(escapedString);
        }

        Console.WriteLine("All pages done successfully.");

    }


    // Helper method to extract a file name from a URL
    static string GetFileNameFromUrl(string url)
    {
        Uri uri = new Uri(url);
        return Path.GetFileName(uri.LocalPath);
    }
}
