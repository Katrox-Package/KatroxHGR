using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using CSVector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Katrox
{
    public static class RayTrace
    {
        public static bool TraceShape(CCSPlayerController player, [NotNullWhen(true)] out TraceResult<CBaseEntity>? result, ulong mask = ~0UL)
        {
            result = null;

            var pPawn = player.PlayerPawn.Value;
            var basePawn = player.Pawn.Value;

            if (basePawn?.CBodyComponent?.SceneNode == null || pPawn == null || !pPawn.IsValid)
                return false;

            var pawnPosition = basePawn.CBodyComponent.SceneNode.AbsOrigin ?? Katrox.VEC_ZERO;
            var pawnAngles = pPawn.EyeAngles ?? Katrox.ANGLE_ZERO;

            var traceResult = TraceShape(pawnPosition, pawnAngles, mask);
            if (traceResult.HasValue)
            {
                var endPos = traceResult.Value.EndPos.ToCSVector();
                var hitEntity = traceResult.Value.GetHitEntityInstance<CBaseEntity>();

                result = new(endPos, hitEntity);
                return true;
            }

            return false;
        }

        public readonly struct TraceResult<T> where T : CEntityInstance
        {
            public CSVector EndPos { get; }
            public T? HitEntity { get; }

            public TraceResult(CSVector endPos, T? hitEntity)
            {
                EndPos = endPos;
                HitEntity = hitEntity;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate bool TraceShapeDelegate(
            nint gameTraceManager,
            nint vecStart,
            nint vecEnd,
            nint skip,
            ulong mask,
            byte a6,
            GameTrace* pGameTrace
        );

        private static TraceShapeDelegate? _traceShape;

        private static readonly nint TraceFunc =
            NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("TraceFunc"));

        private static readonly nint GameTraceManager =
            NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("GameTraceManager"));

        private static readonly Lazy<nint> GameTraceManagerAddress = new(() =>
            Address.GetAbsoluteAddress(GameTraceManager, 3, 7));

        private static unsafe GameTrace? TraceShape(CSVector origin, QAngle angles, ulong mask = ~0UL)
        {
            if (TraceFunc == 0 || GameTraceManager == 0)
                return null;

            _traceShape ??= Marshal.GetDelegateForFunctionPointer<TraceShapeDelegate>(TraceFunc);

            var forward = new CSVector();
            NativeAPI.AngleVectors(angles.Handle, forward.Handle, IntPtr.Zero, IntPtr.Zero);

            var start = new CSVector(
                origin.X + forward.X * 50,
                origin.Y + forward.Y * 50,
                origin.Z + forward.Z * 50 + 64
            );

            var end = new CSVector(
                origin.X + forward.X * 8192,
                origin.Y + forward.Y * 8192,
                origin.Z + forward.Z * 8192
            );

            var trace = stackalloc GameTrace[1];

            bool result = _traceShape(
                *(nint*)GameTraceManagerAddress.Value,
                start.Handle,
                end.Handle,
                IntPtr.Zero,
                mask,
                4,
                trace
            );

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

        public static nint GetCallAddress(nint address)
        {
            return GetAbsoluteAddress(address, 1, 5);
        }
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
        [FieldOffset(0x00)] public void* Surface;
        [FieldOffset(0x08)] public void* HitEntity;
        [FieldOffset(0x10)] public TraceHitboxData* HitboxData;
        [FieldOffset(0x50)] public uint Contents;
        [FieldOffset(0x78)] public Vector3 StartPos;
        [FieldOffset(0x84)] public Vector3 EndPos;
        [FieldOffset(0x90)] public Vector3 Normal;
        [FieldOffset(0x9C)] public Vector3 Position;
        [FieldOffset(0xAC)] public float Fraction;
        [FieldOffset(0xB6)] public bool AllSolid;

        public T? GetHitEntityInstance<T>() where T : CEntityInstance // thanks to byali
        {
            if (HitEntity == null)
                return null;

            var handle = new nint(HitEntity);
            return Activator.CreateInstance(typeof(T), handle) as T;
        }
    }
}
