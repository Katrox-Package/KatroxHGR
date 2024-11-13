using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using CSVector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Katrox
{
	public partial class Katrox
	{
		public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		public static string GetSigFor(string func)
		{
			var isWin = IsWindows;
			switch (func)
			{
				case "TraceFunc":
					return isWin
						? "4C 8B DC 49 89 5B ? 49 89 6B ? 49 89 73 ? 57 41 56 41 57 48 81 EC ? ? ? ? 0F 57 C0"
						: "48 B8 ? ? ? ? ? ? ? ? 55 48 89 E5 41 57 41 56 49 89 D6 41 55";

				case "GameTraceManager":
					return isWin
						? "48 8B 0D ? ? ? ? 48 8D 45 ? 48 89 44 24 ? 4C 8D 44 24 ? C7 44 24 ? ? ? ? ? 48 8D 54 24 ? 4C 8B CB"
						: "48 8D 05 ? ? ? ? 4C 89 E7 F3 0F 11 95";

				default:
					return string.Empty;
			}
		}

		public static bool CustomRayTrace(CCSPlayerController player, [NotNullWhen(true)] out CSVector? endPos)
		{
			endPos = null;

			var pPawn = player.PlayerPawn.Value;
			var pawn = player.Pawn.Value;
			if (pawn?.CBodyComponent?.SceneNode == null || pPawn == null)
			{
				return false;
			}

			var pos = pawn.CBodyComponent.SceneNode.AbsOrigin ?? VEC_ZERO;
			var angles = pPawn.EyeAngles ?? ANGLE_ZERO;

			var result = RayTrace.TraceShape(pos, angles, ~0UL);

			if (result.HasValue)
			{
				endPos = result.Value.EndPos.ToCSVector();
				return true;
			}
			else
			{
				_Logger?.LogError("Trace failed: result value null");
			}
			return false;
		}
	}

	public class RayTrace
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private unsafe delegate bool TraceShapeDelegate(nint gameTraceManager, nint vecStart, nint vecEnd, nint skip, ulong mask, byte a6, GameTrace* pGameTrace);

		private static TraceShapeDelegate? _traceShape;

		private static readonly nint TraceFunc = NativeAPI.FindSignature(Addresses.ServerPath, Katrox.GetSigFor("TraceFunc"));
		private static readonly nint GameTraceManager = NativeAPI.FindSignature(Addresses.ServerPath, Katrox.GetSigFor("GameTraceManager"));

		public static unsafe GameTrace? TraceShape(CSVector? origin, QAngle angles, ulong mask)
		{
			if (TraceFunc == 0 || GameTraceManager == 0)
			{
				Katrox._Logger?.LogError("TraceFunc or GameTraceManager is 0");
				return null;
			}

			if (origin == null) return null;

			var gameTraceManagerAddress = Address.GetAbsoluteAddress(GameTraceManager, 3, 7);
			_traceShape ??= Marshal.GetDelegateForFunctionPointer<TraceShapeDelegate>(TraceFunc);

			var forward = new CSVector();
			NativeAPI.AngleVectors(angles.Handle, forward.Handle, IntPtr.Zero, IntPtr.Zero);

			var _origin = new CSVector(origin.X, origin.Y, origin.Z);
			var endOrigin = new CSVector(
				_origin.X + forward.X * 8192,
				_origin.Y + forward.Y * 8192,
				_origin.Z + forward.Z * 8192
			);

			_origin.X += forward.X * 50;
			_origin.Y += forward.Y * 50;
			_origin.Z += forward.Z * 50 + 64;

			var trace = stackalloc GameTrace[1];

			bool result = _traceShape(*(nint*)gameTraceManagerAddress, _origin.Handle, endOrigin.Handle, IntPtr.Zero, mask, 4, trace);

			return result ? trace[0] : null;
		}
	}

	internal static class Address
	{
		public static unsafe nint GetAbsoluteAddress(nint addr, nint offset, int size)
		{
			if (addr == 0) return 0;
			int code = *(int*)(addr + offset);
			return addr + code + size;
		}

		public static nint GetCallAddress(nint address) => GetAbsoluteAddress(address, 1, 5);
	}

	[StructLayout(LayoutKind.Explicit, Size = 0x44)]
	public unsafe struct TraceHitboxData
	{
		[FieldOffset(0x38)] public int HitGroup;
		[FieldOffset(0x40)] public int HitboxId;
	}

	[StructLayout(LayoutKind.Explicit, Size = 0xB8)]
	public unsafe struct GameTrace
	{
		[FieldOffset(0)] public void* Surface;
		[FieldOffset(0x8)] public void* HitEntity;
		[FieldOffset(0x10)] public TraceHitboxData* HitboxData;
		[FieldOffset(0x50)] public uint Contents;
		[FieldOffset(0x78)] public Vector3 StartPos;
		[FieldOffset(0x84)] public Vector3 EndPos;
		[FieldOffset(0x90)] public Vector3 Normal;
		[FieldOffset(0x9C)] public Vector3 Position;
		[FieldOffset(0xAC)] public float Fraction;
		[FieldOffset(0xB6)] public bool AllSolid;
	}

}
