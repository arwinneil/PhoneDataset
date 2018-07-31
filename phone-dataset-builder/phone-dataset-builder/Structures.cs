namespace phone_dataset_builder
{
    public struct phone_brand
    {
        public string brand;
        public string model_no;
        public string url;

        // Constructor:
        public phone_brand(string brand, string model_no, string url)
        {
            this.brand = brand;
            this.model_no = model_no;
            this.url = url;
        }
    }

    public struct phone_model
    {
        public string model;
        public string url;

        // Constructor:
        public phone_model(string model, string url)
        {
            this.model = model;
            this.url = url;
        }
    }

    public struct specs
    {
        public string network_technology;
        public string twoG_bands;
        public string threeG_bands;
        public string fourG_bands;
        public string network_speed;
        public string GPRS;
        public string EDGE;
        public string announced;
        public string status;
        public string dimentions;
        public string weight_g;
        public string weight_oz;
        public string SIM;
        public string display_type;
        public string display_size;
        public string display_resolution;
        public string OS;
        public string CPU;
        public string Chipset;
        public string GPU;
        public string memory_card;
        public string internal_memory;
        public string RAM;
        public string primary_camera;
        public string secondary_camera;
        public string loud_speaker;
        public string audio_jack;
        public string WLAN;
        public string bluetooth;
        public string GPS;
        public string NFC;
        public string radio;
        public string USB;
        public string sensors;
        public string battery;
        public string colors;
        public string price_group;
        public string img_url;
        public string test_display;
        public string test_performance;
        public string test_loudspeaker;
        public string test_audio_quality;
        public string test_battery_life;

        // Constructor:
        public specs(string network_technology,
         string twoG_bands,
         string threeG_bands,
         string fourG_bands,
         string network_speed,
         string GPRS,
         string EDGE,
         string announced,
         string status,
         string dimentions,
         string weight_g,
         string weight_oz,
         string SIM,
         string display_type,
         string display_size,
         string display_resolution,
         string OS,
         string CPU,
         string Chipset,
         string GPU,
         string memory_card,
         string internal_memory,
         string RAM,
         string primary_camera,
         string secondary_camera,
         string loud_speaker,
         string audio_jack,
         string WLAN,
         string bluetooth,
         string GPS,
         string NFC,
         string radio,
         string USB,
         string sensors,
         string battery,
         string colors,
         string price_group,
         string img_url,
         string test_display,
         string test_performance,
         string test_loudspeaker,
         string test_audio_quality,
         string test_battery_life
            )
        {
            this.network_technology = network_technology;
            this.twoG_bands = twoG_bands;
            this.fourG_bands = fourG_bands;
            this.threeG_bands = threeG_bands;
            this.network_speed = network_speed;
            this.GPRS = GPRS;
            this.EDGE = EDGE;
            this.announced = announced;
            this.status = status;
            this.dimentions = dimentions;
            this.weight_g = weight_g;
            this.weight_oz = weight_oz;
            this.SIM = SIM;
            this.display_type = display_type;
            this.display_size = display_size;
            this.display_resolution = display_resolution;
            this.OS = OS;
            this.CPU = CPU;
            this.Chipset = Chipset;
            this.GPU = GPU;
            this.memory_card = memory_card;
            this.internal_memory = internal_memory;
            this.RAM = RAM;
            this.primary_camera = primary_camera;
            this.secondary_camera = secondary_camera;
            this.loud_speaker = loud_speaker;
            this.audio_jack = audio_jack;
            this.WLAN = WLAN;
            this.bluetooth = bluetooth;
            this.GPS = GPS;
            this.NFC = NFC;
            this.radio = radio;
            this.USB = USB;
            this.sensors = sensors;
            this.battery = battery;
            this.colors = colors;
            this.price_group = price_group;
            this.img_url = img_url;

            this.test_display = test_display;
            this.test_performance = test_performance;
            this.test_loudspeaker = test_loudspeaker;
            this.test_audio_quality = test_audio_quality;
            this.test_battery_life = test_battery_life;
        }
    }
}