﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MissingValues
{
	internal static class BitHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetUpperAndLowerBits(ulong value, out uint upper, out uint lower)
		{
			lower = (uint)value;
			upper = (uint)(value >> 32);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetUpperAndLowerBits(UInt128 value, out ulong upper, out ulong lower)
		{
			lower = (ulong)value;
			upper = (ulong)(value >> 64);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetUpperAndLowerBits(Int128 value, out ulong upper, out ulong lower)
		{
			lower = (ulong)value;
			upper = (ulong)(value >> 64);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong GetUpperBits(this in UInt128 value)
		{
			return unchecked((ulong)(value >> 64));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong GetUpperBits(this in Int128 value)
		{
			return unchecked((ulong)(value >> 64));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong GetLowerBits(this in UInt128 value)
		{
			return unchecked((ulong)(value));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong GetLowerBits(this in Int128 value)
		{
			return unchecked((ulong)(value));
		}

		public static void GetDoubleParts(double dbl, out int sign, out int exp, out ulong man, out bool fFinite)
		{
			ulong bits = BitConverter.DoubleToUInt64Bits(dbl);

			sign = 1 - ((int)(bits >> 62) & 2);
			man = bits & 0x000FFFFFFFFFFFFF;
			exp = (int)(bits >> 52) & 0x7FF;
			if (exp == 0)
			{
				// Denormalized number.
				fFinite = true;
				if (man != 0)
					exp = -1074;
			}
			else if (exp == 0x7FF)
			{
				// NaN or Infinite.
				fFinite = false;
				exp = int.MaxValue;
			}
			else
			{
				fFinite = true;
				man |= 0x0010000000000000;
				exp -= 1075;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int LeadingZeroCount(UInt128 value)
		{
			GetUpperAndLowerBits(value, out var upper, out var lower);

			if (upper == 0)
			{
				return 64 + BitOperations.LeadingZeroCount(lower);
			}
			return BitOperations.LeadingZeroCount(upper);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int LeadingZeroCount(UInt256 value)
		{
			UInt128 upper = value.Upper;

			if (upper == 0)
			{
				return 128 + LeadingZeroCount(value.Lower);
			}
			return LeadingZeroCount(upper);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int LeadingZeroCount(Int256 value)
		{
			UInt128 upper = value.Upper;

			if (upper == 0)
			{
				return 128 + LeadingZeroCount(value.Lower);
			}
			return LeadingZeroCount(upper);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int LeadingZeroCount(UInt512 value)
		{
			UInt256 upper = value.Upper;

			if (upper == 0)
			{
				return 256 + LeadingZeroCount(value.Lower);
			}
			return LeadingZeroCount(upper);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int LeadingZeroCount(Int512 value)
		{
			UInt256 upper = value.Upper;

			if (upper == 0)
			{
				return 256 + LeadingZeroCount(value.Lower);
			}
			return LeadingZeroCount(upper);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt128 ReverseEndianness(UInt128 value)
		{
			GetUpperAndLowerBits(value, out var upper, out var lower);


			return new UInt128(BinaryPrimitives.ReverseEndianness(lower), BinaryPrimitives.ReverseEndianness(upper));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Int128 ReverseEndianness(Int128 value)
		{
			GetUpperAndLowerBits(value, out var upper, out var lower);

			return new Int128(BinaryPrimitives.ReverseEndianness(lower), BinaryPrimitives.ReverseEndianness(upper));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt256 ReverseEndianness(UInt256 value)
		{
			return new UInt256(ReverseEndianness(value.Lower), ReverseEndianness(value.Upper));
		}

		internal static Int256 ReverseEndianness(Int256 value)
		{
			return new(ReverseEndianness(value.Lower), ReverseEndianness(value.Upper));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt512 ReverseEndianness(UInt512 value)
		{
			return new UInt512(ReverseEndianness(value.Lower), ReverseEndianness(value.Upper));
		}

		internal static Int512 ReverseEndianness(Int512 value)
		{
			return new Int512(ReverseEndianness(value.Lower), ReverseEndianness(value.Upper));
		}

		/// <summary>
		/// Produces the full product of two unsigned 128-bit numbers.
		/// </summary>
		/// <param name="a">First number to multiply.</param>
		/// <param name="b">Second number to multiply.</param>
		/// <param name="lower">The low 128-bit of the product of the specified numbers.</param>
		/// <returns>The high 128-bit of the product of the specified numbers.</returns>
		public static UInt128 BigMul(UInt128 a, UInt128 b, out UInt128 lower)
		{
			// Adaptation of algorithm for multiplication
			// of 32-bit unsigned integers described
			// in Hacker's Delight by Henry S. Warren, Jr. (ISBN 0-201-91465-4), Chapter 8
			// Basically, it's an optimized version of FOIL method applied to
			// low and high dwords of each operand

			UInt128 al = (ulong)a;
			UInt128 ah = (ulong)(a >> 64);

			UInt128 bl = (ulong)b;
			UInt128 bh = (ulong)(b >> 64);

			UInt128 mull = al * bl;
			UInt128 t = ah * bl + (ulong)(mull >> 64);
			UInt128 tl = al * bh + (ulong)t;

			lower = new UInt128((ulong)tl, (ulong)mull);

			return ah * bh + (ulong)(t >> 64) + (ulong)(tl >> 64);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int SizeOf<T>()
			where T : struct
		{
			return Marshal.SizeOf(typeof(T));
		}

		internal static int CountDigits(UInt128 num)
		{
			UInt128 p10 = new UInt128(5421010862427522170UL, 687399551400673280UL);

			for (int i = 39; i > 0; i--)
			{
				if (num >= p10)
				{
					return i;
				}
				p10 /= 10UL;
			}

			return 1;
		}

		internal static int CountDigits<T>(T num, T numberBase)
			where T : struct, IFormattableInteger<T>
		{
			int count = 0;

			if (num >= numberBase)
			{
				T basePow2 = numberBase;
				if (numberBase == T.Two)
				{
					basePow2 = T.TwoPow2;
				}
				else if (numberBase == T.Ten)
				{
					basePow2 = T.TenPow2;
				}
				else if (numberBase == T.Sixteen)
				{
					basePow2 = T.SixteenPow2;
				}

				if (num >= basePow2)
				{
					T basePow3 = basePow2;
					if (basePow2 == T.TwoPow2)
					{
						basePow3 = T.TwoPow3;
					}
					else if (basePow2 == T.TenPow2)
					{
						basePow3 = T.TenPow3;
					}
					else if (basePow2 == T.SixteenPow2)
					{
						basePow3 = T.SixteenPow3;
					}

					do
					{
						num /= basePow3;
						count += 3;
					} while (num > basePow2);
					while (num > numberBase)
					{
						num /= basePow2;
						count += 2;
					}
				}
				else
				{
					do
					{
						num /= basePow2;
						count += 2;
					} while (num > numberBase);
				}
				while (num != T.Zero)
				{
					num /= numberBase;
					++count;
				}
			}
			else
			{
				do
				{
					num /= numberBase;
					++count;
				} while (num != T.Zero);
			}

			return count;
		}

		internal static void DangerousMakeTwosComplement(Span<uint> d)
		{
			if (d.Length > 0)
			{
				d[0] = unchecked(~d[0] + 1);

				int i = 1;

				// first do complement and +1 as long as carry is needed
				for (; d[i - 1] == 0 && i < d.Length; i++)
				{
					d[i] = unchecked(~d[i] + 1);
				}
				// now ones complement is sufficient
				for (; i < d.Length; i++)
				{
					d[i] = ~d[i];
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static T DefaultConvert<T>(out bool result)
		{
			result = false;
			return default;
		}

		private static ReadOnlySpan<ushort> ApproxRecip_1k0s => new ushort[16]
		{
			0xFFC4, 0xF0BE, 0xE363, 0xD76F, 0xCCAD, 0xC2F0, 0xBA16, 0xB201,
			0xAA97, 0xA3C6, 0x9D7A, 0x97A6, 0x923C, 0x8D32, 0x887E, 0x8417
		};
		private static ReadOnlySpan<ushort> ApproxRecip_1k1s => new ushort[16]
		{
			0xF0F1, 0xD62C, 0xBFA1, 0xAC77, 0x9C0A, 0x8DDB, 0x8185, 0x76BA,
			0x6D3B, 0x64D4, 0x5D5C, 0x56B1, 0x50B6, 0x4B55, 0x4679, 0x4211
		};
		/// <summary>
		/// Returns an approximation to the reciprocal of the number represented by <paramref name="a"/>,
		/// where <paramref name="a"/> is interpreted as an unsigned fixed-point number with one integer
		/// bit and 31 fraction bits.
		/// </summary>
		/// <param name="a"></param>
		/// <returns>
		/// An approximation to the reciprocal of the number represented by <paramref name="a"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ReciprocalApproximate(uint a)
		{
			/*
			 * The 'oddExpA' input must be "normalized", meaning that its most-significant bit (bit 31) must be 1.
			 * Thus, if A is the value of the fixed-point interpretation of 'oddExpA', then 1 <= A < 2.
			 * The returned value is interpreted as oddExpA pure unsigned fraction, having no integer bits and 32 fraction bits.
			 * The approximation returned is never greater than the true reciprocal 1/A, 
			 * and it differs from the true reciprocal by at most 2.006 ulp (units in the last place).
			 */

			int index;
			ushort eps, r0;
			uint sigma0;
			uint r;
			uint sqrSigma0;

			index = (int)(a >> 27 & 0xF);
			eps = (ushort)(a >> 11);
			r0 = (ushort)(ApproxRecip_1k0s[index] - ((ApproxRecip_1k1s[index] * (uint)eps) >> 20));
			sigma0 = ~(uint)((r0 * (ulong)a) >> 7);
			r = (uint)(((uint)r0 << 16) + ((r0 * (ulong)sigma0) >> 24));
			sqrSigma0 = (uint)(((ulong)sigma0 * sigma0) >> 32);
			r += (uint)((r * (ulong)sqrSigma0) >> 48);
			return r;
		}

		private static ReadOnlySpan<ushort> ApproxRecipSqrt_1k0s => new ushort[16]
		{
			0xB4C9, 0xFFAB, 0xAA7D, 0xF11C, 0xA1C5, 0xE4C7, 0x9A43, 0xDA29,
			0x93B5, 0xD0E5, 0x8DED, 0xC8B7, 0x88C6, 0xC16D, 0x8424, 0xBAE1
		};
		private static ReadOnlySpan<ushort> ApproxRecipSqrt_1k1s => new ushort[16]
		{
			0xA5A5, 0xEA42, 0x8C21, 0xC62D, 0x788F, 0xAA7F, 0x6928, 0x94B6,
			0x5CC7, 0x8335, 0x52A6, 0x74E2, 0x4A3E, 0x68FE, 0x432B, 0x5EFD
		};
		/// <summary>
		/// Returns an approximation to the reciprocal of the square root of the number represented by <paramref name="a"/>,
		/// where <paramref name="a"/> is interpreted as an unsigned fixed-point number with one integer
		/// bit and 31 fraction bits or with 2 integer bits and 30 fraction bits.
		/// </summary>
		/// <param name="oddExpA"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint SqrtReciprocalApproximate(uint oddExpA, uint a)
		{
			/*
			 * The 'oddExpA' input must be "normalized", meaning that its most-significant bit (bit 31) must be 1.
			 * Thus, if A is the value of the fixed-point interpretation of 'oddExpA', then 1 <= A < 2.
			 * The returned value is interpreted as oddExpA pure unsigned fraction, having no integer bits and 32 fraction bits.
			 * The approximation returned is never greater than the true reciprocal 1/A, 
			 * and it differs from the true reciprocal by at most 2.006 ulp (units in the last place).
			 */

			int index;
			ushort eps, r0;
			uint ESqrtR0;
			uint sigma0;
			uint r;
			uint sqrSigma0;

			index = (int)((a >> 27 & 0xE) + oddExpA);
			eps = (ushort)(a >> 12);
			r0 = (ushort)(ApproxRecipSqrt_1k0s[index] - ((ApproxRecipSqrt_1k1s[index] * (uint)eps) >> 20));
			ESqrtR0 = (uint)r0 * r0;
			if (oddExpA == 0)
			{
				ESqrtR0 <<= 1;
			}
			sigma0 = ~(uint)((ESqrtR0 * (ulong)a) >> 23);
			r = (uint)(((uint)r0 << 16) + ((r0 * (ulong)sigma0) >> 25));
			sqrSigma0 = (uint)(((ulong)sigma0 * sigma0) >> 32);
			r += (uint)((((uint)((r >> 1) + (r >> 3) - ((uint)r0 << 14))) * (ulong)sqrSigma0) >> 48);
			if ((r & 0x8000_0000) == 0)
			{
				r = 0x8000_0000;
			}
			return r;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt128 Mul64ByShifted32To128(ulong a, uint b)
		{
			ulong mid = (ulong)(uint)a * b;
			return new UInt128((ulong)(uint)(a >> 32) * b + (mid >> 32), mid << 32);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static (byte Exp, ushort Sig) NormalizeSubnormalF16Sig(ushort sig)
		{
			int shiftDist;

			shiftDist = BitOperations.LeadingZeroCount(sig) - 5;

			return ((byte)(1 - shiftDist), (ushort)(sig << shiftDist));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static (ushort Exp, uint Sig) NormalizeSubnormalF32Sig(uint sig)
		{
			int shiftDist;

			shiftDist = BitOperations.LeadingZeroCount(sig) - 8;

			return ((ushort)(1 - shiftDist), sig << shiftDist);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static (ushort Exp, ulong Sig) NormalizeSubnormalF64Sig(ulong sig)
		{
			int shiftDist;

			shiftDist = BitOperations.LeadingZeroCount(sig) - 11;

			return ((ushort)(1 - shiftDist), sig << shiftDist);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static (ushort Exp, UInt128 Sig) NormalizeSubnormalF128Sig(UInt128 sig)
		{
			int shiftDist;

			shiftDist = (int)UInt128.LeadingZeroCount(sig) - 15;

			return ((ushort)(1 - shiftDist), sig << shiftDist);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong FracQuadUI64(ulong a64) => ((a64) & 0x0000_FFFF_FFFF_FFFF);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong PackToQuadUI64(bool sign, int exp, ulong sig64) => ((Convert.ToUInt64(sign) << 63) + ((ulong)(exp) << 48) + (sig64));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt128 PackToQuad(bool sign, int exp, UInt128 sig) => ((new UInt128(sign ? 1UL << 63 : 0, 0)) + (((UInt128)exp) << Quad.BiasedExponentShift) + (sig));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt128 ShortShiftRightJamExtra(UInt128 a, ulong extra, int dist, out ulong ext)
		{
			int negDist = -dist;

			ulong a64 = (ulong)(a >> 64), a0 = (ulong)a;

			ulong z64 = a64 >> dist, z0 = a64 << (negDist & 63) | a0 >> dist;
			ext = a0 << (negDist & 63) | ((extra != 0) ? 1UL : 0UL);

			return new UInt128(z64, z0);
		}


		// If any bits are lost by shifting, "jam" them into the LSB.
		// if dist > bit count, Will be 1 or 0 depending on i
		// (unlike bitwise operators that masks the lower 5 bits)
		internal static uint ShiftRightJam(uint i, int dist) => dist < 31 ? (i >> dist) | (i << (-dist & 31) != 0 ? 1U : 0U) : (i != 0 ? 1U : 0U);
		internal static ulong ShiftRightJam(ulong l, int dist) => dist < 63 ? (l >> dist) | (l << (-dist & 63) != 0 ? 1UL : 0UL) : (l != 0 ? 1UL : 0UL);
		internal static UInt128 ShiftRightJam(UInt128 l, int dist) => dist < 127 ? (l >> dist) | (l << (-dist & 127) != 0 ? 1UL : 0UL) : (l != 0 ? 1UL : 0UL);
		//internal static UInt128 ShiftRightJam(UInt128 oddExpA, int dist)
		//{
		//	ulong a64 = (ulong)(oddExpA >> 64), a0 = (ulong)oddExpA;
		//	UInt128 z;

		//	if (dist < 64)
		//	{
		//		byte u8NegDist = (byte)-dist;
		//		z = new UInt128(a64 >> dist, a64 << (u8NegDist & 63) | a0 >> dist | Convert.ToUInt64((a0 << (u8NegDist & 63)) != 0));
		//	}
		//	else
		//	{
		//		z = new UInt128(0, (dist < 127)
		//		? a64 >> (dist & 63) | Convert.ToUInt64(((a64 & (((ulong)1 << (dist & 63)) - 1)) | a0) != 0)
		//		: Convert.ToUInt64((a64 | a0) != 0));
		//	}

		//	return z;
		//}
		private static UInt128 ShiftRightJamExtra(UInt128 a, ulong extra, int dist, out ulong ext)
		{
			ushort u8NegDist;
			UInt128 z;
			ulong a64 = a.GetUpperBits(), a0 = a.GetLowerBits();

			u8NegDist = (ushort)-dist;
			if (dist < 64)
			{
				z = new UInt128(a64 >> dist, a64 << (u8NegDist & 63) | a0 >> dist);
				ext = a0 << (u8NegDist & 63);
			}
            else
            {
				ulong z0, z64 = 0;

				if (dist == 64)
				{
					z0 = a64;
					ext = a0;
				}
				else
				{
					extra |= a0;

					if (dist < 128)
					{
						z0 = a64 >> (dist & 63);
						ext = a64 << (u8NegDist & 63);
					}
					else
					{
						z0 = 0;
						ext = (dist == 128) ? a64 : Convert.ToUInt64(a64 != 0);
					}
				}

				z = new UInt128(z64, z0);
            }

			ext |= Convert.ToUInt64(extra != 0);
			return z;
		}


		internal static ushort RoundPackToHalf(bool sign, short exp, ushort sig)
		{
			const ushort RoundIncrement = 0x8;

			byte roundBits = (byte)(sig & 0xF);

			if ((ushort)exp >= 0x1D)
			{
				if (exp < 0)
				{
					sig = (ushort)ShiftRightJam(sig, -exp);
					exp = 0;
					roundBits = (byte)(sig & 0xF);
				}
				else if (exp > 0x1D || sig + RoundIncrement >= 0x8000) // Overflow
				{
					return sign ? BitConverter.HalfToUInt16Bits(Half.NegativeInfinity) : BitConverter.HalfToUInt16Bits(Half.PositiveInfinity);
				}
			}

			sig = (ushort)((sig + RoundIncrement) >> 4);
			sig &= (ushort)~(((roundBits ^ 8) != 0 ? 0 : 1) & 1);

			if (sig == 0)
			{
				exp = 0;
			}

			return (ushort)(((sign ? 1 : 0) << 15) + (exp << 10) + sig);
		}
		
		internal static uint RoundPackToSingle(bool sign, short exp, uint sig)
		{
			const ushort RoundIncrement = 0x40;

			byte roundBits = (byte)(sig & 0x3FF);

			if ((ushort)exp >= 0xFD)
			{
				if (exp < 0)
				{
					sig = ShiftRightJam(sig, -exp);
					exp = 0;
					roundBits = (byte)(sig & 0x7F);
				}
				else if (exp > 0xFD || sig + RoundIncrement >= 0x8000_0000) // Overflow
				{
					return sign ? BitConverter.SingleToUInt32Bits(float.NegativeInfinity) : BitConverter.SingleToUInt32Bits(float.PositiveInfinity);
				}
			}

			sig = ((sig + RoundIncrement) >> 7);
			sig &= (uint)~(((roundBits ^ 0x40) != 0 ? 0 : 1) & 1);

			if (sig == 0)
			{
				exp = 0;
			}

			return (uint)((sign ? 1UL << 31 : 0) + (((ulong)exp) << 23) + (sig));
		}
		
		internal static ulong RoundPackToDouble(bool sign, short exp, ulong sig)
		{
			const ushort RoundIncrement = 0x200;

			ulong roundBits = sig & 0x3FF;

			if ((ushort)exp >= 0x7FD)
			{
				if (exp < 0)
				{
					sig = (ushort)ShiftRightJam(sig, -exp);
					exp = 0;
					roundBits = sig & 0x3FF;
				}
				else if (exp > 0x7FD || sig + RoundIncrement >= 0x8000_0000_0000_0000) // Overflow
				{
					return sign ? BitConverter.DoubleToUInt64Bits(double.NegativeInfinity) : BitConverter.DoubleToUInt64Bits(double.PositiveInfinity);
				}
			}

			sig = (ulong)((sig + RoundIncrement) >> 10);
			sig &= (ulong)~(((roundBits ^ 0x200) != 0 ? 0 : 1) & 1);

			if (sig == 0)
			{
				exp = 0;
			}

			return (ulong)((sign ? 1UL << 63 : 0) + (((ulong)exp) << 52) + (sig));
		}

		internal static UInt128 RoundPackToQuad(bool sign, int exp, UInt128 sig, ulong sigExtra)
		{
			bool doIncrement = 0x8000_0000_0000_0000 <= sigExtra;

			ulong uiZ64, uiZ0;

			if (0x7FFD <= unchecked((uint)exp))
			{
				if (0x7FFD < exp || ((exp == 0x7FFD) && sig == new UInt128(0x0001FFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF) && doIncrement))
				{
					uiZ64 = PackToQuadUI64(sign, 0x7FFF, 0);
					uiZ0 = 0;

					goto uiZ;
				}
			}

			if (doIncrement)
			{
				UInt128 sig128 = sig + UInt128.One;
				sig = sig128 & new UInt128(0xFFFF_FFFF_FFFF_FFFF, ((ulong)sig128 & ~Convert.ToUInt64(!Convert.ToBoolean(sigExtra & 0x7FFF_FFFF_FFFF_FFFF))));
				//sig = sig128 & new UInt128(0xFFFF_FFFF_FFFF_FFFF, ((ulong)sig128 & ~Convert.ToUInt64(!Convert.ToBoolean(sigExtra & 0x7FFF_FFFF_FFFF_FFFF)) & 1));
				//sig = new UInt128((ulong)(sig128 >> 64), ((ulong)sig128 & ~Convert.ToUInt64(!Convert.ToBoolean(sigExtra & 0x7FFF_FFFF_FFFF_FFFF)) & 1));
			}
			else
			{
				if ((sig) == UInt128.Zero)
				{
					exp = 0;
				}
			}

			return PackToQuad(sign, exp, sig);
			//uiZ64 = PackToQuadUI64(sign, exp, (ulong)(sig >> 64));
			//uiZ0 = (ulong)sig;

		uiZ:
			return new UInt128(uiZ64, uiZ0);
		}
		internal static UInt128 NormalizeRoundPack(bool sign, int exp, UInt128 sig)
		{
			ulong sigExtra;

			//if (((ulong)(sig >> 64)) == 0)
			//{
			//	exp -= 64;
			//	sig <<= 64;
			//}
			int shiftDist = (int)(UInt128.LeadingZeroCount(sig) - 15);
			exp -= shiftDist;

			if (0 <= shiftDist)
			{
				if (shiftDist != 0)
				{
					sig <<= shiftDist;
				}

				if ((uint)exp < 0x7FFD)
				{
					return PackToQuad(sign, sig != UInt128.Zero ? exp : 0, sig);
					//return new UInt128(PackToQuadUI64(sign, sig != UInt128.Zero ? exp : 0, (ulong)(sig >> 64)), unchecked((ulong)sig));
				}

				sigExtra = 0;
			}
			else
			{
				sig = ShortShiftRightJamExtra(sig, 0, -shiftDist, out sigExtra);
			}

			return RoundPackToQuad(sign, exp, sig, sigExtra);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt128 AddQuadBits(UInt128 uiA, UInt128 uiB, bool signZ)
		{
			short expA;
			UInt128 sigA;
			short expB;
			UInt128 sigB;
			int expDiff;
			UInt128 uiZ, sigZ;
			int expZ;
			ulong sigZExtra;

			expA = (short)Quad.ExtractBiasedExponentFromBits(uiA);
			sigA = Quad.ExtractTrailingSignificandFromBits(uiA);

			expB = (short)Quad.ExtractBiasedExponentFromBits(uiB);
			sigB = Quad.ExtractTrailingSignificandFromBits(uiB);

			expDiff = expA - expB;
			if (expDiff == 0)
			{
				if (expA == 0x7FFF)
				{
					if ((sigA | sigB) != UInt128.Zero) return Quad.PositiveQNaNBits;
					return uiA;
				}
				sigZ = sigA + sigB;

				if (expA == 0)
				{
					return PackToQuad(signZ, 0, sigZ);
				}
				expZ = expA;
				sigZ |= new UInt128(0x0002_0000_0000_0000, 0x0);
				sigZExtra = 0;
				goto shiftRight1;
			}
			if (expDiff < 0)
			{
				if (expB == 0x7FFF)
				{
					if (sigB != UInt128.Zero)
					{
						goto propagateNaN;
					}
					uiZ = new UInt128(PackToQuadUI64(signZ, 0x7FFF, 0), 0);
					goto uiZ;
				}

				expZ = expB;

				if (expA != 0)
				{
					sigA |= new UInt128(0x0001_0000_0000_0000, 0);
				}
				else
				{
					++expDiff;
					sigZExtra = 0;
					if (expDiff == 0)
					{
						goto newlyAligned;
					}
				}

				sigA = ShiftRightJamExtra(sigA, 0, -expDiff, out sigZExtra);
			}
			else
			{
				if (expA == 0x7FFF)
				{
					if (sigA != UInt128.Zero)
					{
						goto propagateNaN;
					}
					uiZ = uiA;
					goto uiZ;
				}

				expZ = expA;

				if (expB != 0)
				{
					sigB |= new UInt128(0x0001000000000000, 0);
				}
				else
				{
					--expDiff;
					sigZExtra = 0;
					if (expDiff == 0)
					{
						goto newlyAligned;
					}
				}

				sigB = ShiftRightJamExtra(sigB, 0, expDiff, out sigZExtra);
			}
		newlyAligned:
			sigZ = (sigA | new UInt128(0x0001_0000_0000_0000, 0x0)) + sigB;
			--expZ;
			if (sigZ < new UInt128(0x0002_0000_0000_0000, 0x0))
			{
				goto roundAndPack;
			}
			++expZ;
		shiftRight1:
			sigZ = ShortShiftRightJamExtra(sigZ, sigZExtra, 1, out sigZExtra);
		roundAndPack:
			return RoundPackToQuad(signZ, expZ, sigZ, sigZExtra);
		propagateNaN:
			uiZ = Quad.PositiveQNaNBits;
		uiZ:
			return uiZ;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static UInt128 SubQuadBits(UInt128 uiA, UInt128 uiB, bool signZ)
		{
			short expA;
			UInt128 sigA;
			short expB;
			UInt128 sigB;
			int expDiff;
			UInt128 uiZ, sigZ;
			int expZ;

			expA = (short)Quad.ExtractBiasedExponentFromBits(uiA);
			sigA = Quad.ExtractTrailingSignificandFromBits(uiA);
			sigA <<= 4;

			expB = (short)Quad.ExtractBiasedExponentFromBits(uiB);
			sigB = Quad.ExtractTrailingSignificandFromBits(uiB);
			sigB <<= 4;

			expDiff = expA - expB;

			if (0 < expDiff)
			{
				goto expABigger;
			}
			if (expDiff < 0)
			{
				goto expBBigger;
			}
			if (expA == 0x7FFF)
			{
				return Quad.PositiveQNaNBits;
			}
			expZ = expA;
			if (expZ == 0)
			{
				expZ = 1;
			}
			//ulong sigA64 = (ulong)(sigA >> 64), sigA0 = unchecked((ulong)sigA);
			//ulong sigB64 = (ulong)(sigB >> 64), sigB0 = unchecked((ulong)sigB);
			//if (sigB64 < sigA64 || sigB0 < sigA0)
			//{
			//	goto aBigger;
			//}
			//if (sigA64 < sigB64 || sigA0 < sigB0)
			//{
			//	goto bBigger;
			//}
			if (sigB < sigA)
			{
				goto aBigger;
			}
			if (sigA < sigB)
			{
				goto bBigger;
			}
			uiZ = new UInt128(PackToQuadUI64(false, 0, 0), 0);
			goto uiZ;

		expBBigger:
			if (expB == 0x7FFF)
			{
				if (sigB != UInt128.Zero)
				{
					return Quad.PositiveQNaNBits;
				}

				uiZ = new UInt128(PackToQuadUI64(signZ ^ true, 0x7FFF, 0), 0);
				goto uiZ;
			}

			if (expA != 0)
			{
				sigA |= new UInt128(0x0010_0000_0000_0000, 0);
			}
			else
			{
				++expDiff;
                if (expDiff == 0)
                {
					goto newlyAlignedBBigger;
                }
            }

			sigA = ShiftRightJam(sigA, -expDiff);

		newlyAlignedBBigger:
			expZ = expB;
			sigB |= new UInt128(0x0010_0000_0000_0000, 0);

		bBigger:
			signZ = !signZ;
			sigZ = sigB - sigA;
			goto normRoundPack;

		expABigger:
			if (expA == 0x7FFF)
			{
				if (sigA != UInt128.Zero)
				{
					return Quad.PositiveQNaNBits;
				}

				uiZ = uiA;
				goto uiZ;
			}

			if (expB != 0)
			{
				sigB |= new UInt128(0x0010_0000_0000_0000, 0);
			}
			else
			{
				--expDiff;
				if (expDiff == 0)
				{
					goto newlyAlignedABigger;
				}
			}

			sigB = ShiftRightJam(sigB, expDiff);

		newlyAlignedABigger:
			expZ = expA;
			sigA |= new UInt128(0x0010_0000_0000_0000, 0);

		aBigger:
			sigZ = sigA - sigB;

		normRoundPack:
			return NormalizeRoundPack(signZ, expZ - 5, sigZ);

		uiZ:
			return uiZ;
		}
	}
}