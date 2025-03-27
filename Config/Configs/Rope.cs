namespace Katrox
{
    public class Rope
    {
        public string UsePermission { get; set; } = "@css/ban";
        public string GivePermission { get; set; } = "@css/ban";

        public string Rope1 { get; set; } = "rope1";
        public string Rope0 { get; set; } = "rope0";
        public string[] GiveTempRope { get; set; } = ["giverope", "ropegive"];
        public string[] RemoveTempRope { get; set; } = ["deleterope", "removerope"];
    }
}
