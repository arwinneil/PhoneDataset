using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace phone_dataset_builder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            createDataset();

            Console.WriteLine("Retrieving phone brands from GSM arena... \n");
            List<phone_brand> PhoneBrands = getBrandList();

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("\n\nRetrived " + PhoneBrands.Count + " phone brands\n");
            Console.ResetColor();
            Console.WriteLine("Retrieving phone models by brand... \n");
            Console.Write("Input starting point :");

            string st = Console.ReadLine();

            bool canstart = false;

            foreach (phone_brand Phone in PhoneBrands)
            {
                // selective brand start

                if (Phone.brand == st)
                {
                    canstart = true;
                }

                if (canstart)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Phone.brand);
                    Console.ResetColor();
                    Console.Write(" : " + Phone.model_no + " reported devices ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" url:" + Phone.url + "\n");
                    Console.ResetColor();

                    List<phone_model> Model = getModelList(Phone.url, false, Phone.model_no);

                    int writecount = 0;
                    foreach (phone_model model in Model)
                    {
                        specs Specs = getSpecs(model.url, model.model);
                        Console.Write("\rWriting specifications to dataset :" + ++writecount + "/" + Model.Count + "...");
                        writeSpecs(Phone.brand, model.model, Specs);
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done!\n");
                    Console.ResetColor();
                }
            }
        }

        private static List<phone_brand> getBrandList()
        {
            List<phone_brand> PhoneBrands = new List<phone_brand>();

            WebClient client = new WebClient();

            StreamWriter rawhtml = new StreamWriter("RawListPage.html");
            rawhtml.WriteLine(client.DownloadString("http://www.gsmarena.com/makers.php3"));
            rawhtml.Close();

            StreamReader sr = new StreamReader("RawListPage.html");

            string line;
            bool table = false;
            while ((line = sr.ReadLine()) != null)

            {
                if (line.IndexOf("</table>") == 0)
                    break;

                if (table == true && line != "")

                {
                    string temp_brand = line.Substring((line.IndexOf(".php>") + 5), ((line.IndexOf("<br>")) - (line.IndexOf(".php>") + 5)));
                    string temp_model_no = line.Substring((line.IndexOf("<span>") + 6), ((line.IndexOf(" devices")) - (line.IndexOf("<span>") + 6)));
                    string temp_url = "http://www.gsmarena.com/" + line.Substring((line.IndexOf("href=") + 5), ((line.IndexOf(".php>") + 4) - (line.IndexOf("href=") + 5)));

                    phone_brand Phone = new phone_brand(temp_brand, temp_model_no, temp_url);

                    PhoneBrands.Add(Phone);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Phone.brand);
                    Console.ResetColor();
                    Console.Write(";");
                    ;
                }

                if (line.IndexOf("<table>") == 0)
                    table = true;
            }
            sr.Close();
            File.Delete("RawListPage.html");
            return PhoneBrands;
        }

        private static List<phone_model> getModelList(string url, bool isRecursion, string model_no)
        {
            List<phone_model> PhoneModels = new List<phone_model>();

            List<string> navigation_pages = new List<string>();

            WebClient client = new WebClient();

            StreamWriter rawhtml = new StreamWriter("RawModelPage.html");
            rawhtml.WriteLine(client.DownloadString(url));

            rawhtml.Close();

            StreamReader sr = new StreamReader("RawModelPage.html");

            string line = null;
            bool model_class_found = false;
            bool page_class_found = false;
            string raw_models = null;

            while ((line = sr.ReadLine()) != null)

            {
                if (line == "<div class=\"makers\">")
                    model_class_found = true;

                if ((line.IndexOf("<li>") == 0) && (model_class_found == true))
                {
                    raw_models = line;
                    model_class_found = false;
                }

                if (!isRecursion) //index search pages only if not recursion
                {
                    if (line == "<div class=\"nav-pages\">")
                        page_class_found = true;

                    if ((line.IndexOf("<strong>") == 0) && (page_class_found == true))
                    {
                        do
                        {
                            if (line.Contains("href") == true)
                            {
                                string temp_page = line.Substring((line.IndexOf("href=\"") + 6), ((line.IndexOf(".php") + 4) - (line.IndexOf("href=\"") + 6)));

                                navigation_pages.Add("http://www.gsmarena.com/" + temp_page);
                            }

                            line = line.Remove(0, (line.IndexOf(".php") + 4));
                        } while (line.Contains("href") == true);
                    }
                }
            }

            if (page_class_found)
            {
                Console.Write("Recursively fetching phone models from ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine((navigation_pages.Count + 1) + " result pages");
                Console.ResetColor();
            }

            do

            {
                if (raw_models.Contains("<span>") == true)
                {
                    string temp_url = raw_models.Substring((raw_models.IndexOf("href=\"") + 6), ((raw_models.IndexOf(".php") + 4) - (raw_models.IndexOf("href=\"") + 6)));

                    string temp_model = raw_models.Substring((raw_models.IndexOf("<span>") + 6), ((raw_models.IndexOf("</span>") - (raw_models.IndexOf("<span>") + 6))));

                    temp_url = "http://www.gsmarena.com/" + temp_url;

                    phone_model model = new phone_model(temp_model, temp_url);
                    PhoneModels.Add(model);
                }

                raw_models = raw_models.Remove(0, (raw_models.IndexOf("</span>") + 4));
            } while (raw_models.Contains("<span>") == true);

            sr.Close();
            File.Delete("RawModelPage.html");

            if (!isRecursion)

            {
                Console.Write("\rPopulating model list :" + PhoneModels.Count + "/" + model_no + "...");

                foreach (string result_url in navigation_pages)
                {
                    PhoneModels.AddRange(getModelList(result_url, true, model_no)); //Recursicely get phone models from other result pages and add to list

                    Console.Write("\rPopulating model list :" + PhoneModels.Count + "/" + model_no + "...");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done!");
                Console.ResetColor();
            }

            return PhoneModels;
        }

        private static String getContent(ref StreamReader sr)
        {
            String line = "";
            while (!line.Contains("</td>"))
            {
                line += sr.ReadLine();
            }
            line = new Regex("<[^>]*>").Replace(line, "");
            line = line.Replace("\n", "");
            line = line.Replace("\r", "");
            line = line.Replace(",", "|");
            return line;
        }

        private static specs getSpecs(string url, string model)
        {
            WebClient client = new WebClient();

            bool tryagain = true;
            while (tryagain)
            {
                try
                {
                    StreamWriter rawhtml = new StreamWriter("RawSpecs.html");
                    rawhtml.WriteLine(client.DownloadString(url));
                    rawhtml.Close();
                    tryagain = false;
                }
                catch
                {
                    tryagain = true;
                    Console.WriteLine("retryin");
                }
            }

            StreamReader sr = new StreamReader("RawSpecs.html");

            string line = null;
            bool specs_list_found = false;

            string network_technology = "";
            string twoG_bands = "";
            string fourG_bands = "";
            string threeG_bands = "";
            string network_speed = "";
            string GPRS = "";
            string EDGE = "";
            string announced = "";
            string status = "";
            string dimentions = "";
            string weight_g = "";
            string weight_oz = "";
            string SIM = "";
            string display_type = "";
            string display_resolution = "";
            string display_size = "";
            string OS = "";
            string CPU = "";
            string Chipset = "";
            string GPU = "";
            string memory_card = "";
            string internal_memory = "";
            string RAM = "";
            string primary_camera = "";
            string secondary_camera = "";
            string loud_speaker = "";
            string audio_jack = "";
            string WLAN = "";
            string bluetooth = "";
            string GPS = "";
            string NFC = "";
            string radio = "";
            string USB = "";
            string sensors = "";
            string battery = "";
            string colors = "";
            string price_group = "";
            string img_url = "";
            string test_display = "";
            string test_performance = "";
            string test_loudspeaker = "";
            string test_audio_quality = "";
            string test_battery_life = "";
            while ((line = sr.ReadLine()) != null)
            {
                if (line.IndexOf("<div id=\"specs-list\">") == 0)
                    specs_list_found = true;

                if (line.IndexOf("<p class=\"note\">") == 0)
                    break;

                if (line.IndexOf("HISTORY_ITEM_IMAGE") == 0)
                {
                    line = line.Remove(0, line.IndexOf("\"") + 1);
                    line = line.Substring(0, line.IndexOf("\""));
                    img_url = line;

                    continue;
                }

                if ((line.IndexOf("Battery") > -1) && (line.IndexOf("th") > -1) && (line.IndexOf("nbsp") == -1) && (line.IndexOf("lab") == -1))
                {
                    line = sr.ReadLine();

                    line = sr.ReadLine();

                    line = line.Remove(0, line.IndexOf(">") + 1);

                    line = line.Remove(line.IndexOf("</td>"), ((line.Length) - (line.IndexOf("<"))));

                    battery = line;

                    continue;
                }

                if (specs_list_found)
                {
                    if ((line.IndexOf("ttl") > -1) && (line.IndexOf("nbsp") == -1) && (line.IndexOf("lab") == -1))
                    {
                        if (line.IndexOf("Technology") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            network_technology = line;

                            continue;
                        }

                        if (line.IndexOf("2G bands") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            twoG_bands = line;

                            continue;
                        }

                        if (line.IndexOf("3G bands") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            threeG_bands = line;

                            continue;
                        }

                        if (line.IndexOf("4G bands") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            fourG_bands = line;

                            continue;
                        }

                        if (line.IndexOf("Speed") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            network_speed = line;

                            continue;
                        }

                        if (line.IndexOf("GPRS") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            GPRS = line;

                            continue;
                        }

                        if (line.IndexOf("EDGE") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            EDGE = line;

                            continue;
                        }

                        if (line.IndexOf("Announced") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            announced = line;

                            continue;
                        }

                        if (line.IndexOf("Status") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            status = line;

                            continue;
                        }

                        if (line.IndexOf("Dimensions") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            dimentions = line;

                            continue;
                        }

                        if (line.IndexOf("Weight") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            if (line.IndexOf("g") > -1)
                            {
                                weight_g = line.Substring(0, line.IndexOf(" "));

                                line = line.Remove(0, line.IndexOf("(") + 1);
                                weight_oz = line.Substring(0, line.IndexOf(" "));
                            }

                            continue;
                        }

                        if (line.IndexOf("SIM") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            SIM = line;

                            continue;
                        }

                        if (line.IndexOf("Type") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            display_type = line;

                            continue;
                        }

                        if (line.IndexOf("Size") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            display_size = line;

                            continue;
                        }

                        if (line.IndexOf("Resolution") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            display_resolution = line;

                            continue;
                        }

                        if (line.IndexOf("OS") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            OS = line;

                            continue;
                        }

                        if (line.IndexOf("Chipset") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            Chipset = line;

                            continue;
                        }

                        if (line.IndexOf("CPU") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            CPU = line;

                            continue;
                        }

                        if (line.IndexOf("GPU") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            GPU = line;

                            continue;
                        }

                        if (line.IndexOf("Card slot") > -1)
                        {
                            do
                            {
                                line = sr.ReadLine();
                            } while (line.IndexOf("nfo") == -1);

                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            memory_card = line;

                            continue;
                        }

                        if (line.IndexOf("Internal") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            if (line.IndexOf(",") > -1)
                            {
                                if ((line.Substring(0, line.IndexOf(",")).IndexOf("RAM") > -1))
                                {
                                    RAM = line.Substring(0, line.IndexOf(","));
                                    internal_memory = line.Remove(0, line.IndexOf(",") + 2);
                                }
                                else
                                {
                                    internal_memory = line.Substring(0, line.IndexOf(","));
                                    RAM = line.Remove(0, line.IndexOf(",") + 2);
                                }

                                continue;
                            }
                            else if (line.IndexOf(";") > -1)
                            {
                                if ((line.Substring(0, line.IndexOf(";")).IndexOf("RAM") > -1))
                                {
                                    RAM = line.Substring(0, line.IndexOf(";"));
                                    internal_memory = line.Remove(0, line.IndexOf(";") + 2);
                                }
                                else
                                {
                                    internal_memory = line.Substring(0, line.IndexOf(";"));
                                    RAM = line.Remove(0, line.IndexOf(";") + 2);
                                }

                                continue;
                            }
                            else if (line.IndexOf("+") > -1)
                            {
                                if ((line.Substring(0, line.IndexOf("+")).IndexOf("RAM") > -1))
                                {
                                    RAM = line.Substring(0, line.IndexOf("+"));
                                    internal_memory = line.Remove(0, line.IndexOf("+") + 2);
                                }
                                else
                                {
                                    internal_memory = line.Substring(0, line.IndexOf("+"));
                                    RAM = line.Remove(0, line.IndexOf("+") + 2);
                                }

                                continue;
                            }
                            else if (line.IndexOf("|") > -1)
                            {
                                if ((line.Substring(0, line.IndexOf("|")).IndexOf("RAM") > -1))

                                {
                                    Console.WriteLine(line);

                                    RAM = line.Substring(0, line.IndexOf("|"));
                                    Console.WriteLine(RAM);

                                    internal_memory = line.Remove(0, line.IndexOf("|") + 2);
                                    Console.WriteLine(internal_memory);
                                }
                                else
                                {
                                    internal_memory = line.Substring(0, line.IndexOf("|"));
                                    Console.WriteLine(internal_memory);

                                    RAM = line.Remove(0, line.IndexOf("|") + 2);
                                    Console.WriteLine(RAM);
                                }

                                continue;
                            }
                            else
                            {
                                if (line.IndexOf("RAM") > -1)
                                    RAM = line;
                                else
                                    internal_memory = line;
                            }
                        }

                        if (line.IndexOf("Primary") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);

                            if (line.IndexOf("<") < 0)
                                line = line + sr.ReadLine();

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            primary_camera = line;

                            continue;
                        }

                        if (line.IndexOf("Secondary") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            secondary_camera = line;

                            continue;
                        }
                        if (line.IndexOf("Loudspeaker") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            loud_speaker = line;

                            continue;
                        }

                        if (line.IndexOf("3.5mm jack") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            audio_jack = line;

                            continue;
                        }

                        if (line.IndexOf("Loudspeaker") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            loud_speaker = line;

                            continue;
                        }

                        if (line.IndexOf("WLAN") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            WLAN = line;

                            continue;
                        }

                        if (line.IndexOf("Bluetooth") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            bluetooth = line;

                            continue;
                        }
                        if (line.IndexOf("GPS") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            GPS = line;

                            continue;
                        }

                        if (line.IndexOf("NFC") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            NFC = line;

                            continue;
                        }

                        if (line.IndexOf("Radio") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            radio = line;

                            continue;
                        }

                        if (line.IndexOf("USB") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            USB = line;

                            continue;
                        }

                        if (line.IndexOf("Sensors") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            sensors = line;

                            continue;
                        }

                        if (line.IndexOf("Colors") > -1)
                        {
                            line = sr.ReadLine();
                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            colors = line;

                            continue;
                        }


                        if (line.IndexOf("Price") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            line = line.Remove(0, 6);

                            line = line.Remove(line.IndexOf(" "));

                            price_group = line;

                            continue;
                        }

                        //Default specs parsing template

                        //line = line.Remove(0, line.IndexOf(">") + 1);

                        //line = line.Remove(0, line.IndexOf(">") + 1);

                        //line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                        //Console.Write(line + ",");
                    }

                    if (line.IndexOf("lab_tests") > -1)
                    {

                        if (line.IndexOf("Performance") > -1)
                        {
                            test_performance = getContent(ref sr);

                            continue;
                        }

                        if (line.IndexOf("Display") > -1)
                        {
                            test_display = getContent(ref sr);

                            continue;
                        }

                        if (line.IndexOf("Loudspeaker") > -1)
                        {
                            test_loudspeaker = getContent(ref sr);

                            continue;
                        }

                        if (line.IndexOf("Audio quality") > -1)
                        {
                            test_audio_quality = getContent(ref sr);

                            continue;
                        }

                        if (line.IndexOf("Battery life") > -1)
                        {
                            test_battery_life = getContent(ref sr);

                            continue;
                        }
                    }
                }
            }

            sr.Close();
            File.Delete("RawSpecs.html");

            specs Specs = new specs(network_technology,
             twoG_bands,
             threeG_bands,
             fourG_bands,
             network_speed,
             GPRS,
             EDGE,
             announced,
             status,
             dimentions,
             weight_g,
             weight_oz,
             SIM,
             display_type,
             display_resolution,
             display_size,
             OS,
             CPU,
             Chipset,
             GPU,
             memory_card,
             internal_memory,
             RAM,
             primary_camera,
             secondary_camera,
             loud_speaker,
             audio_jack,
             WLAN,
             bluetooth,
             GPS,
             NFC,
             radio,
             USB,
             sensors,
             battery,
             colors,
             price_group,
             img_url,
              test_display,
              test_performance,
              test_loudspeaker,
              test_audio_quality,
              test_battery_life
                );

            return Specs;
        }

        private static void createDataset()
        {
            bool tryagain = true;
            while (tryagain)
            {
                try
                {
                    StreamWriter dataset = new StreamWriter("phone_dataset.csv");
                    dataset.WriteLine("brand,model,network_technology,2G_bands,3G_bands,4G_bands,network_speed,GPRS,EDGE,announced,status,dimentions,weight_g,weight_oz,SIM,display_type,display_resolution,display_size,OS,CPU,Chipset,GPU,memory_card,internal_memory,RAM,primary_camera,secondary_camera,loud_speaker,audio_jack,WLAN,bluetooth,GPS,NFC,radio,USB,sensors,battery,colors,approx_price_EUR,img_url,test_display,test_performance,test_loudspeaker,test_audio_quality,test_battery_life");


                    dataset.Close();
                    tryagain = false;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The file \"phone_dataset.csv\" cannot access because it is being used by another process. Please close any program that might be using it then press ENTER...");
                    Console.ResetColor();
                    Console.ReadKey();
                };
            }
        }

        private static void writeSpecs(string brand, string model, specs Specs)
        {
            bool tryagain = true;
            while (tryagain)
            {
                try
                {
                    StreamWriter test = new StreamWriter("phone_dataset.csv", true);

                    test.Close();
                    tryagain = false;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The file \"phone_dataset.csv\" cannot access because it is being used by another process. Please close any program that might be using it then press ENTER...");
                    Console.ResetColor();
                    Console.ReadKey();
                };
            }
            StreamWriter dataset = new StreamWriter("phone_dataset.csv", true);

            Specs.network_technology = Specs.network_technology.Replace(',', '|');
            Specs.twoG_bands = Specs.twoG_bands.Replace(',', '|');
            Specs.fourG_bands = Specs.fourG_bands.Replace(',', '|');
            Specs.threeG_bands = Specs.threeG_bands.Replace(',', '|');
            Specs.network_speed = Specs.network_speed.Replace(',', ' ');
            Specs.GPRS = Specs.GPRS.Replace(',', '|');
            Specs.EDGE = Specs.EDGE.Replace(',', '|');
            Specs.announced = Specs.announced.Replace(',', ' ');
            Specs.status = Specs.status.Replace(',', ' ');
            Specs.dimentions = Specs.dimentions.Replace(',', '|');

            //Format weights to numeric
            Specs.weight_g = Specs.weight_g.Replace("g", string.Empty);
            Specs.weight_oz = Specs.weight_oz.Replace("oz", string.Empty);
            Specs.weight_oz = Specs.weight_oz.Replace("z", string.Empty);
            Specs.weight_oz = Specs.weight_oz.Replace("o", string.Empty);

            Specs.weight_g = Specs.weight_g.Trim();
            Specs.weight_oz = Specs.weight_oz.Trim();

            Specs.SIM = Specs.SIM.Replace(',', '|');
            Specs.display_type = Specs.display_type.Replace(',', ' ');
            Specs.display_size = Specs.display_size.Replace(',', '|');
            Specs.display_resolution = Specs.display_resolution.Replace(',', '|');
            Specs.OS = Specs.OS.Replace(',', '|');
            Specs.CPU = Specs.CPU.Replace(',', '|');
            Specs.Chipset = Specs.Chipset.Replace(',', '|');
            Specs.GPU = Specs.GPU.Replace(',', '|');
            Specs.memory_card = Specs.memory_card.Replace(',', ' ');
            Specs.internal_memory = Specs.internal_memory.Replace(',', '|');
            Specs.RAM = Specs.RAM.Replace(',', '|');
            Specs.primary_camera = Specs.primary_camera.Replace(',', '|');
            Specs.secondary_camera = Specs.secondary_camera.Replace(',', '|');
            Specs.loud_speaker = Specs.loud_speaker.Replace(",", string.Empty);
            Specs.audio_jack = Specs.audio_jack.Replace(',', '|');
            Specs.WLAN = Specs.WLAN.Replace(',', '|');
            Specs.bluetooth = Specs.bluetooth.Replace(',', '|');
            Specs.GPS = Specs.GPS.Replace(",", string.Empty);
            Specs.NFC = Specs.NFC.Replace(',', '|');
            Specs.radio = Specs.radio.Replace(',', '|');
            Specs.USB = Specs.USB.Replace(',', '|');
            Specs.sensors = Specs.sensors.Replace(',', '|');
            Specs.battery = Specs.battery.Replace(',', '|');
            Specs.colors = Specs.colors.Replace(',', '|');
            Specs.price_group = Specs.price_group.Replace(',', '|');
            Specs.img_url = Specs.img_url.Replace(',', '|');

            Specs.test_display = Specs.test_display.Replace(',', '|');
            Specs.test_performance = Specs.test_performance.Replace(',', '|');
            Specs.test_loudspeaker = Specs.test_loudspeaker.Replace(',', '|');
            Specs.test_audio_quality = Specs.test_audio_quality.Replace(',', '|');
            Specs.test_battery_life = Specs.test_battery_life.Replace(',', '|');

            dataset.WriteLine(brand + "," + model + "," + Specs.network_technology + "," + Specs.twoG_bands + "," + Specs.threeG_bands + "," + Specs.fourG_bands + "," + Specs.network_speed + "," + Specs.GPRS + "," + Specs.EDGE + "," + Specs.announced + "," + Specs.status + "," + Specs.dimentions + "," + Specs.weight_g + "," + Specs.weight_oz + "," + Specs.SIM + "," + Specs.display_type + "," + Specs.display_resolution + "," + Specs.display_size + "," + Specs.OS + "," + Specs.CPU + "," + Specs.Chipset + "," + Specs.GPU + "," + Specs.memory_card + "," + Specs.internal_memory + "," + Specs.RAM + "," + Specs.primary_camera + "," + Specs.secondary_camera + "," + Specs.loud_speaker + "," + Specs.audio_jack + "," + Specs.WLAN + "," + Specs.bluetooth + "," + Specs.GPS + "," + Specs.NFC + "," + Specs.radio + "," + Specs.USB + "," + Specs.sensors + "," + Specs.battery + "," + Specs.colors + "," + Specs.price_group 
                              + "," + Specs.img_url + "," + Specs.test_display + "," + Specs.test_performance + "," + Specs.test_loudspeaker + "," + Specs.test_audio_quality + "," + Specs.test_battery_life);

            dataset.Close();
        }
    }
}