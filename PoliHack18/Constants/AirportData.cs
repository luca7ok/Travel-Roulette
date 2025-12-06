namespace PoliHack18.Constants
{
    public static class AirportData
    {
        public class AirportInfo
        {
            public string Code { get; set; } = "";
            public string City { get; set; } = "";
            public string Country { get; set; } = "";
            
            public string DisplayName => $"{City} ({Code})";
        }

        private static readonly Dictionary<string, AirportInfo> _airports = new Dictionary<string, AirportInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // UK
            {"LHR", new AirportInfo { Code = "LHR", City = "London", Country = "United Kingdom" }},
            {"LGW", new AirportInfo { Code = "LGW", City = "London", Country = "United Kingdom" }},
            {"MAN", new AirportInfo { Code = "MAN", City = "Manchester", Country = "United Kingdom" }},
            {"STN", new AirportInfo { Code = "STN", City = "London", Country = "United Kingdom" }},
            {"EDI", new AirportInfo { Code = "EDI", City = "Edinburgh", Country = "United Kingdom" }},
            {"BHX", new AirportInfo { Code = "BHX", City = "Birmingham", Country = "United Kingdom" }},
            {"GLA", new AirportInfo { Code = "GLA", City = "Glasgow", Country = "United Kingdom" }},
            
            // France
            {"CDG", new AirportInfo { Code = "CDG", City = "Paris", Country = "France" }},
            {"ORY", new AirportInfo { Code = "ORY", City = "Paris", Country = "France" }},
            {"NCE", new AirportInfo { Code = "NCE", City = "Nice", Country = "France" }},
            {"LYS", new AirportInfo { Code = "LYS", City = "Lyon", Country = "France" }},
            {"MRS", new AirportInfo { Code = "MRS", City = "Marseille", Country = "France" }},
            {"TLS", new AirportInfo { Code = "TLS", City = "Toulouse", Country = "France" }},
            {"BOD", new AirportInfo { Code = "BOD", City = "Bordeaux", Country = "France" }},
            {"NTE", new AirportInfo { Code = "NTE", City = "Nantes", Country = "France" }},
            
            // Germany
            {"FRA", new AirportInfo { Code = "FRA", City = "Frankfurt", Country = "Germany" }},
            {"MUC", new AirportInfo { Code = "MUC", City = "Munich", Country = "Germany" }},
            {"TXL", new AirportInfo { Code = "TXL", City = "Berlin", Country = "Germany" }},
            {"BER", new AirportInfo { Code = "BER", City = "Berlin", Country = "Germany" }},
            {"DUS", new AirportInfo { Code = "DUS", City = "Düsseldorf", Country = "Germany" }},
            {"HAM", new AirportInfo { Code = "HAM", City = "Hamburg", Country = "Germany" }},
            {"CGN", new AirportInfo { Code = "CGN", City = "Cologne", Country = "Germany" }},
            {"STR", new AirportInfo { Code = "STR", City = "Stuttgart", Country = "Germany" }},
            
            // Spain
            {"MAD", new AirportInfo { Code = "MAD", City = "Madrid", Country = "Spain" }},
            {"BCN", new AirportInfo { Code = "BCN", City = "Barcelona", Country = "Spain" }},
            {"PMI", new AirportInfo { Code = "PMI", City = "Palma de Mallorca", Country = "Spain" }},
            {"AGP", new AirportInfo { Code = "AGP", City = "Málaga", Country = "Spain" }},
            {"SVQ", new AirportInfo { Code = "SVQ", City = "Seville", Country = "Spain" }},
            {"VLC", new AirportInfo { Code = "VLC", City = "Valencia", Country = "Spain" }},
            {"ALC", new AirportInfo { Code = "ALC", City = "Alicante", Country = "Spain" }},
            {"BIO", new AirportInfo { Code = "BIO", City = "Bilbao", Country = "Spain" }},
            
            // Italy
            {"FCO", new AirportInfo { Code = "FCO", City = "Rome", Country = "Italy" }},
            {"MXP", new AirportInfo { Code = "MXP", City = "Milan", Country = "Italy" }},
            {"LIN", new AirportInfo { Code = "LIN", City = "Milan", Country = "Italy" }},
            {"VCE", new AirportInfo { Code = "VCE", City = "Venice", Country = "Italy" }},
            {"NAP", new AirportInfo { Code = "NAP", City = "Naples", Country = "Italy" }},
            {"BGY", new AirportInfo { Code = "BGY", City = "Bergamo", Country = "Italy" }},
            {"BLQ", new AirportInfo { Code = "BLQ", City = "Bologna", Country = "Italy" }},
            {"CTA", new AirportInfo { Code = "CTA", City = "Catania", Country = "Italy" }},
            
            // Netherlands
            {"AMS", new AirportInfo { Code = "AMS", City = "Amsterdam", Country = "Netherlands" }},
            {"EIN", new AirportInfo { Code = "EIN", City = "Eindhoven", Country = "Netherlands" }},
            {"RTM", new AirportInfo { Code = "RTM", City = "Rotterdam", Country = "Netherlands" }},
            
            // Belgium
            {"BRU", new AirportInfo { Code = "BRU", City = "Brussels", Country = "Belgium" }},
            {"CRL", new AirportInfo { Code = "CRL", City = "Charleroi", Country = "Belgium" }},
            
            // Switzerland
            {"ZRH", new AirportInfo { Code = "ZRH", City = "Zurich", Country = "Switzerland" }},
            {"GVA", new AirportInfo { Code = "GVA", City = "Geneva", Country = "Switzerland" }},
            {"BSL", new AirportInfo { Code = "BSL", City = "Basel", Country = "Switzerland" }},
            
            // Austria
            {"VIE", new AirportInfo { Code = "VIE", City = "Vienna", Country = "Austria" }},
            {"SZG", new AirportInfo { Code = "SZG", City = "Salzburg", Country = "Austria" }},
            {"INN", new AirportInfo { Code = "INN", City = "Innsbruck", Country = "Austria" }},
            
            // Portugal
            {"LIS", new AirportInfo { Code = "LIS", City = "Lisbon", Country = "Portugal" }},
            {"OPO", new AirportInfo { Code = "OPO", City = "Porto", Country = "Portugal" }},
            {"FAO", new AirportInfo { Code = "FAO", City = "Faro", Country = "Portugal" }},
            
            // Greece
            {"ATH", new AirportInfo { Code = "ATH", City = "Athens", Country = "Greece" }},
            {"SKG", new AirportInfo { Code = "SKG", City = "Thessaloniki", Country = "Greece" }},
            {"HER", new AirportInfo { Code = "HER", City = "Heraklion", Country = "Greece" }},
            {"RHO", new AirportInfo { Code = "RHO", City = "Rhodes", Country = "Greece" }},
            {"CFU", new AirportInfo { Code = "CFU", City = "Corfu", Country = "Greece" }},
            
            // Ireland
            {"DUB", new AirportInfo { Code = "DUB", City = "Dublin", Country = "Ireland" }},
            {"ORK", new AirportInfo { Code = "ORK", City = "Cork", Country = "Ireland" }},
            {"SNN", new AirportInfo { Code = "SNN", City = "Shannon", Country = "Ireland" }},
            
            // Poland
            {"WAW", new AirportInfo { Code = "WAW", City = "Warsaw", Country = "Poland" }},
            {"KRK", new AirportInfo { Code = "KRK", City = "Kraków", Country = "Poland" }},
            {"GDN", new AirportInfo { Code = "GDN", City = "Gdańsk", Country = "Poland" }},
            {"WRO", new AirportInfo { Code = "WRO", City = "Wrocław", Country = "Poland" }},
            {"KTW", new AirportInfo { Code = "KTW", City = "Katowice", Country = "Poland" }},
            
            // Czech Republic
            {"PRG", new AirportInfo { Code = "PRG", City = "Prague", Country = "Czech Republic" }},
            
            // Hungary
            {"BUD", new AirportInfo { Code = "BUD", City = "Budapest", Country = "Hungary" }},
            
            // Romania
            {"OTP", new AirportInfo { Code = "OTP", City = "Bucharest", Country = "Romania" }},
            {"CLJ", new AirportInfo { Code = "CLJ", City = "Cluj-Napoca", Country = "Romania" }},
            
            // Denmark
            {"CPH", new AirportInfo { Code = "CPH", City = "Copenhagen", Country = "Denmark" }},
            {"BLL", new AirportInfo { Code = "BLL", City = "Billund", Country = "Denmark" }},
            
            // Sweden
            {"ARN", new AirportInfo { Code = "ARN", City = "Stockholm", Country = "Sweden" }},
            {"GOT", new AirportInfo { Code = "GOT", City = "Gothenburg", Country = "Sweden" }},
            {"MMX", new AirportInfo { Code = "MMX", City = "Malmö", Country = "Sweden" }},
            
            // Norway
            {"OSL", new AirportInfo { Code = "OSL", City = "Oslo", Country = "Norway" }},
            {"BGO", new AirportInfo { Code = "BGO", City = "Bergen", Country = "Norway" }},
            {"TRD", new AirportInfo { Code = "TRD", City = "Trondheim", Country = "Norway" }},
            
            // Finland
            {"HEL", new AirportInfo { Code = "HEL", City = "Helsinki", Country = "Finland" }},
            
            // Croatia
            {"ZAG", new AirportInfo { Code = "ZAG", City = "Zagreb", Country = "Croatia" }},
            {"DBV", new AirportInfo { Code = "DBV", City = "Dubrovnik", Country = "Croatia" }},
            {"SPU", new AirportInfo { Code = "SPU", City = "Split", Country = "Croatia" }},
            
            // Iceland
            {"KEF", new AirportInfo { Code = "KEF", City = "Reykjavik", Country = "Iceland" }},
            
            // Bulgaria
            {"SOF", new AirportInfo { Code = "SOF", City = "Sofia", Country = "Bulgaria" }},
            {"VAR", new AirportInfo { Code = "VAR", City = "Varna", Country = "Bulgaria" }},
            {"BOJ", new AirportInfo { Code = "BOJ", City = "Burgas", Country = "Bulgaria" }},
            
            // Malta
            {"MLA", new AirportInfo { Code = "MLA", City = "Valletta", Country = "Malta" }},
            
            // Cyprus
            {"LCA", new AirportInfo { Code = "LCA", City = "Larnaca", Country = "Cyprus" }},
            {"PFO", new AirportInfo { Code = "PFO", City = "Paphos", Country = "Cyprus" }},
            
            // Estonia
            {"TLL", new AirportInfo { Code = "TLL", City = "Tallinn", Country = "Estonia" }},
            
            // Latvia
            {"RIX", new AirportInfo { Code = "RIX", City = "Riga", Country = "Latvia" }},
            
            // Lithuania
            {"VNO", new AirportInfo { Code = "VNO", City = "Vilnius", Country = "Lithuania" }},
            
            // Slovenia
            {"LJU", new AirportInfo { Code = "LJU", City = "Ljubljana", Country = "Slovenia" }},
            
            // Slovakia
            {"BTS", new AirportInfo { Code = "BTS", City = "Bratislava", Country = "Slovakia" }}
        };

        public static HashSet<string> EuropeanAirportCodes => new HashSet<string>(_airports.Keys, StringComparer.OrdinalIgnoreCase);

        public static IEnumerable<AirportInfo> GetAllAirports() => _airports.Values.OrderBy(a => a.City).ThenBy(a => a.Code);

        public static string GetCityName(string airportCode)
        {
            return _airports.TryGetValue(airportCode.ToUpper(), out var info) ? info.City : airportCode;
        }

        public static AirportInfo? GetAirportInfo(string airportCode)
        {
            return _airports.TryGetValue(airportCode.ToUpper(), out var info) ? info : null;
        }

        public static bool IsEuropeanAirport(string airportCode)
        {
            return _airports.ContainsKey(airportCode.ToUpper());
        }
    }
}