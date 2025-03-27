namespace Katrox
{
    public class Grab
    {
        public string UsePermission { get; set; } = "@css/ban";
        public string GivePermission { get; set; } = "@css/ban";

        public string Grab1 { get; set; } = "grab1";
        public string Grab0 { get; set; } = "grab0";
        public string[] GiveTempGrab { get; set; } = ["givegrab", "grabgive"];
        public string[] RemoveTempGrab { get; set; } = ["deletegrab", "removegrab"];
    }
}
