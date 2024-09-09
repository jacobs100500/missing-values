﻿using MissingValues.Info;
using MissingValues.Internals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MissingValues
{
	/// <summary>
	/// Represents a 512-bit signed integer.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	[JsonConverter(typeof(NumberConverter.Int512Converter))]
	[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
	public readonly partial struct Int512
	{
		internal const int Size = 64;

		/// <summary>
		/// Represents the value <c>1</c> of the type.
		/// </summary>
		public static readonly Int512 One = new Int512(0, 0, 0, 0, 0, 0, 0, 1);
		/// <summary>
		/// Represents the largest possible value of the type.
		/// </summary>
		public static readonly Int512 MaxValue = new Int512(_upperMax, _lowerMax);
		/// <summary>
		/// Represents the smallest possible value of the type.
		/// </summary>
		public static readonly Int512 MinValue = new Int512(_upperMin, _lowerMin);
		/// <summary>
		/// Represents the value <c>-1</c> of the type.
		/// </summary>
		public static readonly Int512 NegativeOne = new Int512(_lowerMax, _lowerMax);
		/// <summary>
		/// Represents the value <c>0</c> of the type.
		/// </summary>
		public static readonly Int512 Zero = default;

#if BIGENDIAN
		private readonly ulong _p7;
		private readonly ulong _p6;
		private readonly ulong _p5;
		private readonly ulong _p4;
		private readonly ulong _p3;
		private readonly ulong _p2;
		private readonly ulong _p1;
		private readonly ulong _p0;
#else
		private readonly ulong _p0;
		private readonly ulong _p1;
		private readonly ulong _p2;
		private readonly ulong _p3;
		private readonly ulong _p4;
		private readonly ulong _p5;
		private readonly ulong _p6;
		private readonly ulong _p7;
#endif

		internal UInt256 Lower => new UInt256(_p3, _p2, _p1, _p0);
		internal UInt256 Upper => new UInt256(_p7, _p6, _p5, _p4);
		internal ulong Part0 => _p0;
		internal ulong Part1 => _p1;
		internal ulong Part2 => _p2;
		internal ulong Part3 => _p3;
		internal ulong Part4 => _p4;
		internal ulong Part5 => _p5;
		internal ulong Part6 => _p6;
		internal ulong Part7 => _p7;

		internal Int512(ulong lower)
		{
			_p0 = lower;
			_p1 = 0;
			_p2 = 0;
			_p3 = 0;
			_p4 = 0;
			_p5 = 0;
			_p6 = 0;
			_p7 = 0;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Int512"/> struct.
		/// </summary>
		/// <param name="lower">The lower 256-bits of the 512-bit value.</param>
		public Int512(UInt256 lower) : this(UInt256.Zero, lower)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Int512"/> struct.
		/// </summary>
		/// <param name="upper">The upper 256-bits of the 512-bit value.</param>
		/// <param name="lower">The lower 256-bits of the 512-bit value.</param>
		public Int512(UInt256 upper, UInt256 lower)
		{
			lower.GetLowerParts(out _p1, out _p0);
			lower.GetUpperParts(out _p3, out _p2);
			upper.GetLowerParts(out _p5, out _p4);
			upper.GetUpperParts(out _p7, out _p6);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Int512"/> struct.
		/// </summary>
		/// <param name="uu">The first 128-bits of the 512-bit value.</param>
		/// <param name="ul">The second 128-bits of the 512-bit value.</param>
		/// <param name="lu">The third 128-bits of the 512-bit value.</param>
		/// <param name="ll">The fourth 128-bits of the 512-bit value.</param>
		public Int512(UInt128 uu, UInt128 ul, UInt128 lu, UInt128 ll)
		{
			_p0 = (ulong)ll;
			_p1 = (ulong)(ll >>> 64);
			_p2 = (ulong)lu;
			_p3 = (ulong)(lu >>> 64);
			_p4 = (ulong)ul;
			_p5 = (ulong)(ul >>> 64);
			_p6 = (ulong)uu;
			_p7 = (ulong)(uu >>> 64);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Int512"/> struct.
		/// </summary>
		/// <param name="uuu">The first 64-bits of the 512-bit value.</param>
		/// <param name="uul">The second 64-bits of the 512-bit value.</param>
		/// <param name="ulu">The third 64-bits of the 512-bit value.</param>
		/// <param name="ull">The fourth 64-bits of the 512-bit value.</param>
		/// <param name="luu">The fifth 64-bits of the 512-bit value.</param>
		/// <param name="lul">The sixth 64-bits of the 512-bit value.</param>
		/// <param name="llu">The seventh 64-bits of the 512-bit value.</param>
		/// <param name="lll">The eighth 64-bits of the 512-bit value.</param>
		public Int512(ulong uuu, ulong uul, ulong ulu, ulong ull, ulong luu, ulong lul, ulong llu, ulong lll)
		{
			_p0 = lll;
			_p1 = llu;
			_p2 = lul;
			_p3 = luu;
			_p4 = ull;
			_p5 = ulu;
			_p6 = uul;
			_p7 = uuu;
		}

		/// <inheritdoc/>
		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return obj is Int512 @int && Equals(@int);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return HashCode.Combine(Upper, Lower);
		}

		/// <inheritdoc/>
		public override string? ToString()
		{
			return ToString("D", CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Produces the full product of two signed 512-bit numbers.
		/// </summary>
		/// <param name="left">First number to multiply.</param>
		/// <param name="right">Second number to multiply.</param>
		/// <param name="low">The low 512-bit of the product of the specified numbers.</param>
		/// <returns>The high 512-bit of the product of the specified numbers.</returns>
		public static Int512 BigMul(Int512 left, Int512 right, out Int512 low)
		{
			// This follows the same logic as is used in `long Math.BigMul(long, long, out long)`

			UInt512 upper = UInt512.BigMul((UInt512)left, (UInt512)right, out UInt512 ulower);
			low = (Int512)ulower;
			return (Int512)(upper) - ((left >> 511) & right) - ((right >> 511) & left);
		}

		/// <summary>Parses a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <returns>The result of parsing <paramref name="s" />.</returns>
		/// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
		/// <exception cref="OverflowException"><paramref name="s" /> is not representable by <see cref="Int512"/>.</exception>
		public static Int512 Parse(ReadOnlySpan<char> s)
		{
			return Parse(s, CultureInfo.CurrentCulture);
		}
		/// <summary>Tries to parse a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <param name="result">On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
		/// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
		public static bool TryParse(ReadOnlySpan<char> s, out Int512 result)
		{
			return TryParse(s, CultureInfo.CurrentCulture, out result);
		}

		#region From Int512
		public static explicit operator char(Int512 value) => (char)value._p0;
		public static explicit operator checked char(Int512 value)
		{
			if (value.Upper == UInt256.Zero)
			{
				throw new OverflowException();
			}
			return checked((char)value._p0);
		}
		// Unsigned
		public static explicit operator byte(Int512 value) => (byte)value._p0;
		public static explicit operator checked byte(Int512 value)
		{
			if (value.Upper != UInt256.Zero)
			{
				Thrower.IntegerOverflow();
			}
			return checked((byte)value._p0);
		}
		[CLSCompliant(false)]
		public static explicit operator ushort(Int512 value) => (ushort)value._p0;
		[CLSCompliant(false)]
		public static explicit operator checked ushort(Int512 value)
		{
			if (value.Upper != UInt256.Zero)
			{
				Thrower.IntegerOverflow();
			}
			return checked((ushort)value._p0);
		}
		[CLSCompliant(false)]
		public static explicit operator uint(Int512 value) => (uint)value._p0;
		[CLSCompliant(false)]
		public static explicit operator checked uint(Int512 value)
		{
			if (value.Upper != UInt256.Zero)
			{
				Thrower.IntegerOverflow();
			}
			return checked((uint)value._p0);
		}
		[CLSCompliant(false)]
		public static explicit operator ulong(Int512 value) => (ulong)value._p0;
		[CLSCompliant(false)]
		public static explicit operator checked ulong(Int512 value)
		{
			if (value.Upper != UInt256.Zero)
			{
				Thrower.IntegerOverflow();
			}
			return checked((ulong)value._p0);
		}
		[CLSCompliant(false)]
		public static explicit operator UInt128(Int512 value) => (UInt128)value.Lower;
		[CLSCompliant(false)]
		public static explicit operator checked UInt128(Int512 value)
		{
			if (value.Upper != UInt256.Zero)
			{
				Thrower.IntegerOverflow();
			}
			return checked((UInt128)value.Lower);
		}
		[CLSCompliant(false)]
		public static explicit operator UInt256(Int512 value) => value.Lower;
		[CLSCompliant(false)]
		public static explicit operator checked UInt256(Int512 value)
		{
			if (value.Upper != UInt256.Zero)
			{
				Thrower.IntegerOverflow();
			}
			return value.Lower;
		}
		[CLSCompliant(false)]
		public static explicit operator UInt512(Int512 value) => new(value._p7, value._p6, value._p5, value._p4, value._p3, value._p2, value._p1, value._p0);
		[CLSCompliant(false)]
		public static explicit operator checked UInt512(Int512 value)
		{
			if ((Int256)value.Upper < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(value._p7, value._p6, value._p5, value._p4, value._p3, value._p2, value._p1, value._p0);
		}
		[CLSCompliant(false)]
		public static explicit operator nuint(Int512 value) => (nuint)value._p0;
		[CLSCompliant(false)]
		public static explicit operator checked nuint(Int512 value)
		{
			if (value.Upper != UInt256.Zero)
			{
				Thrower.IntegerOverflow();
			}
			return checked((nuint)value.Lower);
		}
		// Signed
		[CLSCompliant(false)]
		public static explicit operator sbyte(Int512 value) => (sbyte)value._p0;
		[CLSCompliant(false)]
		public static explicit operator checked sbyte(Int512 value)
		{
			if (~value.Upper == 0)
			{
				Int256 lower = (Int256)value.Lower;
				return checked((sbyte)lower);
			}

			if (value.Upper != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((sbyte)value.Lower);
		}
		public static explicit operator short(Int512 value) => (short)value._p0;
		public static explicit operator checked short(Int512 value)
		{
			if (~value.Upper == 0)
			{
				Int256 lower = (Int256)value.Lower;
				return checked((short)lower);
			}

			if (value.Upper != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((short)value.Lower);
		}
		public static explicit operator int(Int512 value) => (int)value._p0;
		public static explicit operator checked int(Int512 value)
		{
			if (~value.Upper == 0)
			{
				Int256 lower = (Int256)value.Lower;
				return checked((int)lower);
			}

			if (value.Upper != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((int)value.Lower);
		}
		public static explicit operator long(Int512 value) => (long)value._p0;
		public static explicit operator checked long(Int512 value)
		{
			if (~value.Upper == 0)
			{
				Int256 lower = (Int256)value.Lower;
				return checked((long)lower);
			}

			if (value.Upper != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((long)value.Lower);
		}
		public static explicit operator Int128(Int512 value) => (Int128)value.Lower;
		public static explicit operator checked Int128(Int512 value)
		{
			if (~value.Upper == 0)
			{
				Int256 lower = (Int256)value.Lower;
				return checked((Int128)lower);
			}

			if (value.Upper != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((Int128)value.Lower);
		}
		public static explicit operator Int256(Int512 value) => (Int256)value.Lower;
		public static explicit operator checked Int256(Int512 value)
		{
			if (~value.Upper == 0)
			{
				return (Int256)value.Lower;
			}

			if (value.Upper != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((Int256)value.Lower);
		}
		public static explicit operator nint(Int512 value) => (nint)value._p0;
		public static explicit operator checked nint(Int512 value)
		{
			if (~value.Upper == 0)
			{
				Int256 lower = (Int256)value.Lower;
				return checked((nint)lower);
			}

			if (value.Upper != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((nint)value.Lower);
		}
		// Floating
		public static explicit operator decimal(Int512 value)
		{
			if (IsNegative(value))
			{
				value = -value;
				return -(decimal)(UInt512)(value);
			}
			return (decimal)(UInt512)(value);
		}
		public static explicit operator Octo(Int512 value)
		{
			if (IsNegative(value))
			{
				value = -value;
				return -(Octo)(UInt512)(value);
			}
			return (Octo)(UInt512)(value);
		}
		public static explicit operator Quad(Int512 value)
		{
			if (IsNegative(value))
			{
				value = -value;
				return -(Quad)(UInt512)(value);
			}
			return (Quad)(UInt512)(value);
		}
		public static explicit operator double(Int512 value)
		{
			if (IsNegative(value))
			{
				value = -value;
				return -(double)(UInt512)(value);
			}
			return (double)(UInt512)(value);
		}
		public static explicit operator Half(Int512 value)
		{
			if (IsNegative(value))
			{
				value = -value;
				return -(Half)(UInt512)(value);
			}
			return (Half)(UInt512)(value);
		}
		public static explicit operator float(Int512 value)
		{
			if (IsNegative(value))
			{
				value = -value;
				return -(float)(UInt512)(value);
			}
			return (float)(UInt512)(value);
		}
		#endregion
		#region To Int512
		//Unsigned
		[CLSCompliant(false)]
		public static explicit operator Int512(byte v) => new Int512(v);
		[CLSCompliant(false)]
		public static explicit operator Int512(ushort v) => new Int512(v);
		[CLSCompliant(false)]
		public static explicit operator Int512(uint v) => new Int512(v);
		[CLSCompliant(false)]
		public static explicit operator Int512(nuint v) => new Int512(v);
		[CLSCompliant(false)]
		public static explicit operator Int512(ulong v) => new Int512(v);
		[CLSCompliant(false)]
		public static explicit operator Int512(UInt128 v) => new Int512(v);
		//Signed
		[CLSCompliant(false)]
		public static implicit operator Int512(sbyte v)
		{
			Int256 lower = v;
			return new((UInt256)(lower >> 255), (UInt256)lower);
		}
		public static implicit operator Int512(short v)
		{
			Int256 lower = v;
			return new((UInt256)(lower >> 255), (UInt256)lower);
		}
		public static implicit operator Int512(int v)
		{
			Int256 lower = v;
			return new((UInt256)(lower >> 255), (UInt256)lower);
		}
		public static implicit operator Int512(nint v)
		{
			Int256 lower = v;
			return new((UInt256)(lower >> 255), (UInt256)lower);
		}
		public static implicit operator Int512(long v)
		{
			Int256 lower = v;
			return new((UInt256)(lower >> 255), (UInt256)lower);
		}
		public static implicit operator Int512(Int128 v)
		{
			Int256 lower = v;
			return new((UInt256)(lower >> 255), (UInt256)lower);
		}
		//Floating
		public static explicit operator Int512(decimal v) => (Int512)(double)v;
		public static explicit operator checked Int512(decimal v) => checked((Int512)(double)v);
		public static explicit operator Int512(double v)
		{
			const double TwoPow255 = 57896044618658097711785492504343953926634992332820282019728792003956564819968.0;

			if (v <= -TwoPow255)
			{
				return MinValue;
			}
			else if (double.IsNaN(v))
			{
				return 0;
			}
			else if (v >= +TwoPow255)
			{
				return MaxValue;
			}

			return ToInt512(v);
		}
		public static explicit operator checked Int512(double v)
		{
			const double TwoPow511 = 57896044618658097711785492504343953926634992332820282019728792003956564819968.0;

			if ((0.0d > v + TwoPow511) || double.IsNaN(v) || (v > +TwoPow511))
			{
				throw new OverflowException();
			}
			if (0.0 == TwoPow511 - v)
			{
				return MaxValue;
			}

			return ToInt512(v);
		}
		public static explicit operator Int512(float v) => (Int512)(double)v;
		public static explicit operator checked Int512(float v) => checked((Int512)(double)v);
		public static explicit operator Int512(Half v) => (Int512)(double)v;
		public static explicit operator checked Int512(Half v) => checked((Int512)(double)v);
		#endregion

		private static Int512 ToInt512(double value)
		{
			const double TwoPow511 = 6703903964971298549787012499102923063739682910296196688861780721860882015036773488400937149083451713845015929093243025426876941405973284973216824503042048.0;

			Debug.Assert(value >= -TwoPow511);
			Debug.Assert(double.IsFinite(value));
			Debug.Assert(TwoPow511 > value);

			bool isNegative = double.IsNegative(value);

			if (isNegative)
			{
				value = -value;
			}

			if (value >= 1.0)
			{
				// In order to convert from double to int512 we first need to extract the signficand,
				// including the implicit leading bit, as a full 512-bit significand. We can then adjust
				// this down to the represented integer by right shifting by the unbiased exponent, taking
				// into account the significand is now represented as 512-bits.

				ulong bits = BitConverter.DoubleToUInt64Bits(value);

				Int512 result = new Int512(new UInt256(new UInt128((bits << 12) >> 1 | 0x8000_0000_0000_0000, 0x0000_0000_0000_0000), UInt128.Zero), UInt256.Zero);
				result >>>= (1023 + 512 - 1 - (int)(bits >> 52));

				if (isNegative)
				{
					return -result;
				}
				return result;
			}
			else
			{
				return Int512.Zero;
			}
		}
	}
}
