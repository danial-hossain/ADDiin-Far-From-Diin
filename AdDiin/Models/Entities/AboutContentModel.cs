namespace AdDiin.Models.Entities
{
    public class AboutContentModel
    {
        public string HeroBadge { get; set; } = "Welcome to";
        public string HeroTitle { get; set; } = "Ad-Diin Mosque & Islamic Center";
        public string HeroDescription { get; set; } = "A spiritual home dedicated to worship, education, transparent charity, and community service. We strive to strengthen Islamic values and foster unity and innovation in our community.";
        
        public List<AboutStatItem> Stats { get; set; } = new()
        {
            new() { Value = "5+", Label = "Daily Prayers" },
            new() { Value = "5,000+", Label = "Community Members" },
            new() { Value = "25+", Label = "Monthly Programs" },
            new() { Value = "12+", Label = "Years Serving" }
        };

        public string MissionTitle { get; set; } = "Our Mission";
        public string MissionDescription { get; set; } = "To establish a vibrant, transparent, and technology-driven Islamic center that serves as a beacon of faith, knowledge, compassion, and sustainable community infrastructure (aligned with SDG 9). We aim to provide a welcoming space for worship, learning, transparent Zakat distribution, and civic empowerment.";

        public string VisionTitle { get; set; } = "Our Vision";
        public string VisionDescription { get; set; } = "To be recognized as a premier smart Islamic institution that nurtures spiritual growth, promotes universal Islamic values, and empowers communities through modern digital tools, charity, and ethical leadership.";

        public string ValuesTitle { get; set; } = "Core Values";
        public string ValuesDescription { get; set; } = "The principles that guide everything we do at Ad-Diin Mosque & Community Platform";

        public List<AboutValueItem> Values { get; set; } = new()
        {
            new() { Title = "Faith (Imaan)", Description = "Encouraging spiritual growth, devotion, and regular congregational worship." },
            new() { Title = "Knowledge (Ilm)", Description = "Promoting authentic Islamic education based on the Holy Qur'an and Sunnah." },
            new() { Title = "Compassion (Rahmah)", Description = "Serving the underprivileged through transparent Zakat, disaster relief, and social welfare." },
            new() { Title = "Innovation (SDG 9)", Description = "Leveraging modern digital infrastructure for transparent operations and AI-powered learning." }
        };

        public string ProgramsTitle { get; set; } = "Our Programs & Initiatives";
        public string ProgramsDescription { get; set; } = "Comprehensive religious, educational, and welfare services offered for the community.";

        public List<AboutProgramItem> Programs { get; set; } = new()
        {
            new() { Title = "Daily Prayers & Jamaat", Description = "Five daily prayers with congregation, Friday Jummah khutbahs, and Ramadan Taraweeh prayers." },
            new() { Title = "Islamic Education & Maktab", Description = "Quran recitation, Tajweed, Hadith study circles, and youth moral education." },
            new() { Title = "Community Welfare & Relief", Description = "Transparent Zakat calculation & distribution, Winter clothes drives, Food packages, and Orphan care." },
            new() { Title = "Smart Milad & Dua Bookings", Description = "Online booking for family Milad, Mahfil, and special supplications with certified Imams." }
        };

        public string CommunityHeadsTitle { get; set; } = "Mosque & Community Leadership";
        public string CommunityHeadsDescription { get; set; } = "Dedicated leadership guiding spiritual administration and community development";

        public List<AboutLeaderItem> CommunityHeads { get; set; } = new()
        {
            new() { Name = "Maulana Dr. Abdur Rahman", Role = "Head Imam & Religious Advisor", Phone = "+880 1711-000001" },
            new() { Name = "Engr. Mojid Uddin", Role = "President, Mosque Committee", Phone = "+880 1812-000002" },
            new() { Name = "Prof. Nurul Islam", Role = "General Secretary & Coordinator", Phone = "+880 1913-000003" }
        };

        public string CtaTitle { get; set; } = "Join Our Blessed Community";
        public string CtaDescription { get; set; } = "Whether you are seeking a peaceful place of worship, Islamic knowledge, or ways to contribute through transparent charity, we welcome you with open arms.";
    }

    public class AboutStatItem
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AboutValueItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AboutProgramItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AboutLeaderItem
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
