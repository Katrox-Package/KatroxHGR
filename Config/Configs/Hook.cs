using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katrox
{
    public class Hook
    {
        public string UsePermission { get; set; } = "@css/ban";
        public string GivePermission { get; set; } = "@css/ban";
        public float DefaultSpeed { get; set; } = 1500f;


        public string Hook1 { get; set; } = "hook1";
        public string Hook0 { get; set; } = "hook0";
        public string[] OpenHookForAll { get; set; } = ["enablehook", "hookenable"];
        public string[] OpenHookForT { get; set; } = ["enablehookt", "hookenablet"];
        public string[] OpenHookForCT { get; set; } = ["enablehookct", "hookenablect"];
        public string[] DisableHookForAll { get; set; } = ["disablehook", "hookdisable"];
        public string[] DisableHookForT { get; set; } = ["disablehookt", "hookdisablet"];
        public string[] DisableHookForCT { get; set; } = ["disablehookct", "hookdisablect"];
        public string[] ChangeHookSpeed { get; set; } = ["hookspeed", "hspeed"];
        public string[] GiveTempHook { get; set; } = ["givehook", "hookgive"];
        public string[] RemoveTempHook { get; set; } = ["deletehook", "removehook"];
    }
}
