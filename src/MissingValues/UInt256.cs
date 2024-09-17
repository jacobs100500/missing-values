﻿using MissingValues.Info;
using MissingValues.Internals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace MissingValues
{
	/// <summary>
	/// Represents a 256-bit unsigned integer.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	[JsonConverter(typeof(NumberConverter.UInt256Converter))]
	[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
	public readonly partial struct UInt256
	{
		internal const int Size = 32;

		/// <summary>
		/// Represents the value <c>1</c> of the type.
		/// </summary>
		public static readonly UInt256 One = new UInt256(0, 0, 0, 1);
		/// <summary>
		/// Represents the largest possible value of the type.
		/// </summary>
		public static readonly UInt256 MaxValue = new UInt256(
			0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF);
		/// <summary>
		/// Represents the smallest possible value of the type.
		/// </summary>
		public static readonly UInt256 MinValue = default;
		/// <summary>
		/// Represents the value <c>0</c> of the type.
		/// </summary>
		public static readonly UInt256 Zero = default;

#if BIGENDIAN
		private readonly ulong _p3;
		private readonly ulong _p2;
		private readonly ulong _p1;
		private readonly ulong _p0;
#else
		private readonly ulong _p0;
		private readonly ulong _p1;
		private readonly ulong _p2;
		private readonly ulong _p3;
#endif

		internal UInt128 Lower => new UInt128(_p1, _p0);
		internal UInt128 Upper => new UInt128(_p3, _p2);
		internal ulong Part0 => _p0;
		internal ulong Part1 => _p1;
		internal ulong Part2 => _p2;
		internal ulong Part3 => _p3;

		/// <summary>
		/// Initializes a new instance of the <see cref="UInt256" /> struct.
		/// </summary>
		/// <param name="u1">The first 64-bits of the 256-bit value.</param>
		/// <param name="u2">The second 64-bits of the 256-bit value.</param>
		/// <param name="l1">The third 64-bits of the 256-bit value.</param>
		/// <param name="l2">The fourth 64-bits of the 256-bit value.</param>
		public UInt256(ulong u1, ulong u2, ulong l1, ulong l2)
		{
			_p3 = u1;
			_p2 = u2;
			_p1 = l1;
			_p0 = l2;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="UInt256" /> struct.
		/// </summary>
		/// <param name="lower">The lower 128-bits of the 256-bit value.</param>
		public UInt256(UInt128 lower) : this(UInt128.Zero, lower)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="UInt256" /> struct.
		/// </summary>
		/// <param name="upper">The upper 128-bits of the 256-bit value.</param>
		/// <param name="lower">The lower 128-bits of the 256-bit value.</param>
		public UInt256(UInt128 upper, UInt128 lower)
		{
			_p0 = (ulong)lower;
			_p1 = (ulong)(lower >> 64);
			_p2 = (ulong)upper;
			_p3 = (ulong)(upper >> 64);
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return ToString("D", CultureInfo.CurrentCulture);
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			return obj is UInt256 @int && Equals(@int);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return HashCode.Combine(_p3, _p2, _p1, _p0);
		}

		/// <summary>
		/// Produces the full product of two unsigned 256-bit numbers.
		/// </summary>
		/// <param name="left">First number to multiply.</param>
		/// <param name="right">Second number to multiply.</param>
		/// <param name="lower">The low 256-bit of the product of the specified numbers.</param>
		/// <returns>The high 256-bit of the product of the specified numbers.</returns>
		public static UInt256 BigMul(UInt256 left, UInt256 right, out UInt256 lower)
		{
			ReadOnlySpan<uint> leftSpan = MemoryMarshal.CreateReadOnlySpan(in Unsafe.As<UInt256, uint>(ref left), (Size / sizeof(uint)) - (BitHelper.LeadingZeroCount(in left) / 32));

			ReadOnlySpan<uint> rightSpan = MemoryMarshal.CreateReadOnlySpan(in Unsafe.As<UInt256, uint>(ref right), (Size / sizeof(uint)) - (BitHelper.LeadingZeroCount(in right) / 32));

			Span<uint> rawBits = stackalloc uint[(Size / sizeof(uint)) * 2];
			rawBits.Clear();

			Calculator.Multiply(leftSpan, rightSpan, rawBits);

			lower = new UInt256(
				(((ulong)rawBits[07] << 32) | rawBits[06]), (((ulong)rawBits[05] << 32) | rawBits[04]),
				(((ulong)rawBits[03] << 32) | rawBits[02]), (((ulong)rawBits[01] << 32) | rawBits[00])
				);
			return new UInt256(
				(((ulong)rawBits[15] << 32) | rawBits[14]), (((ulong)rawBits[13] << 32) | rawBits[12]),
				(((ulong)rawBits[11] << 32) | rawBits[10]), (((ulong)rawBits[09] << 32) | rawBits[08])
				);
		}

		/// <summary>Parses a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <returns>The result of parsing <paramref name="s" />.</returns>
		/// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
		/// <exception cref="OverflowException"><paramref name="s" /> is not representable by <see cref="UInt256"/>.</exception>
		public static UInt256 Parse(ReadOnlySpan<char> s)
		{
			return Parse(s, CultureInfo.CurrentCulture);
		}
		/// <summary>Tries to parse a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <param name="result">On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
		/// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
		public static bool TryParse(ReadOnlySpan<char> s, out UInt256 result)
		{
			return TryParse(s, CultureInfo.CurrentCulture, out result);
		}

		#region From UInt256
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="char"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator char(UInt256 value) => (char)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="char"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="char"/>.</exception>
		public static explicit operator checked char(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((char)value._p0);
		}
		// Unsigned
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="byte"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator byte(UInt256 value) => (byte)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="byte"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="byte"/>.</exception>
		public static explicit operator checked byte(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((byte)value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="ushort"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator ushort(UInt256 value) => (ushort)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="ushort"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="ushort"/>.</exception>
		public static explicit operator checked ushort(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((ushort)value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="uint"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator uint(UInt256 value) => (uint)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="uint"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="uint"/>.</exception>
		public static explicit operator checked uint(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((uint)value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="ulong"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator ulong(UInt256 value) => value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="ulong"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="ulong"/>.</exception>
		public static explicit operator checked ulong(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return value._p0;
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="UInt128"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt128(UInt256 value) => value.Lower;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="UInt128"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt128"/>.</exception>
		public static explicit operator checked UInt128(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return value.Lower;
		}
		/// <summary>
		/// Implicitly converts a <see cref="UInt256" /> value to a <see cref="UInt512"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt512(UInt256 value) => new(value);
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="nuint"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator nuint(UInt256 value) => (nuint)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="nuint"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="nuint"/>.</exception>
		public static explicit operator checked nuint(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return (nuint)value._p0;
		}
		//Signed
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="sbyte"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator sbyte(UInt256 value) => (sbyte)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="sbyte"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="sbyte"/>.</exception>
		public static explicit operator checked sbyte(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((sbyte)value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="short"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator short(UInt256 value) => (short)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="short"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="short"/>.</exception>
		public static explicit operator checked short(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((short)value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="int"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator int(UInt256 value) => (int)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="int"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="int"/>.</exception>
		public static explicit operator checked int(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((int)value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="long"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator long(UInt256 value) => (long)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="long"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="long"/>.</exception>
		public static explicit operator checked long(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return checked((long)value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Int128"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator Int128(UInt256 value) => (Int128)value.Lower;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Int128"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="Int128"/>.</exception>
		public static explicit operator checked Int128(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return (Int128)value.Lower;
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Int256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator Int256(UInt256 value) => new(value._p3, value._p2, value._p1, value._p0);
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Int256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="Int256"/>.</exception>
		public static explicit operator checked Int256(UInt256 value)
		{
			if ((long)value._p3 < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(value._p3, value._p2, value._p1, value._p0);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Int512"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator Int512(UInt256 value) => new(value);
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Int512"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="Int512"/>.</exception>
		public static explicit operator checked Int512(UInt256 value)
		{
			if ((long)value._p3 < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="nint"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator nint(UInt256 value) => (nint)value._p0;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="nint"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="nint"/>.</exception>
		public static explicit operator checked nint(UInt256 value)
		{
			if (value._p3 != 0 || value._p2 != 0 || value._p1 != 0)
			{
				Thrower.IntegerOverflow();
			}
			return (nint)value._p0;
		}
		// Floating
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="decimal"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="decimal"/>.</exception>
		public static explicit operator decimal(UInt256 value)
		{

			if (value.Upper != 0)
			{
				// The default behavior of decimal conversions is to always throw on overflow
				Thrower.IntegerOverflow();
			}

			return (decimal)value.Lower;
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Octo"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator Octo(UInt256 value)
		{
			if (value == UInt256.Zero)
			{
				return Octo.Zero;
			}
			int shiftDist = BitHelper.LeadingZeroCount(in value);
			UInt256 a = (value << shiftDist >> 19); // Significant bits, with bit 237 still intact
			UInt256 b = (value << shiftDist << 237); // Insignificant bits, only relevant for rounding.
			UInt256 m = a + ((b - (b >> 255 & (a == UInt128.Zero ? UInt128.One : UInt128.Zero))) >> 255); // Add one when we need to round up. Break ties to even.
			UInt256 e = (UInt256)(0x400FD - shiftDist); // Exponent plus 262143, minus one, except for zero.
			return Octo.UInt256BitsToOcto((e << 236) + m);
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Quad"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator Quad(UInt256 value)
		{
			if (value.Upper == 0)
			{
				return value._p1 != 0 ? (Quad)value.Lower : (Quad)value._p0;
			}
			else if ((value.Upper >> 32) == UInt128.Zero) // value < (2^224)
			{
				// For values greater than MaxValue but less than 2^224 this takes advantage
				// that we can represent both "halves" of the uint256 within the 112-bit mantissa of
				// a pair of quads.
				Quad twoPow112 = new Quad(0x406F_0000_0000_0000, 0x0000_0000_0000_0000);
				Quad twoPow224 = new Quad(0x40DF_0000_0000_0000, 0x0000_0000_0000_0000);

				UInt128 twoPow112bits = Quad.QuadToUInt128Bits(twoPow112);
				UInt128 twoPow224bits = Quad.QuadToUInt128Bits(twoPow224);

				Quad lower = Quad.UInt128BitsToQuad(twoPow112bits | ((value.Lower << 16) >> 16)) - twoPow112;
				Quad upper = Quad.UInt128BitsToQuad(twoPow224bits | (UInt128)(value >> 112)) - twoPow224;

				return lower + upper;
			}
			else
			{
				// For values greater than 2^224 we basically do the same as before but we need to account
				// for the precision loss that quad will have. As such, the lower value effectively drops the
				// lowest 32 bits and then or's them back to ensure rounding stays correct.

				Quad twoPow144 = new Quad(0x408F_0000_0000_0000, 0x0000_0000_0000_0000);
				Quad twoPow256 = new Quad(0x40FF_0000_0000_0000, 0x0000_0000_0000_0000);

				UInt128 twoPow144bits = Quad.QuadToUInt128Bits(twoPow144);
				UInt128 twoPow256bits = Quad.QuadToUInt128Bits(twoPow256);

				Quad lower = Quad.UInt128BitsToQuad(twoPow144bits | ((UInt128)(value >> 16) >> 16) | (value.Lower & 0xFFFF_FFFF)) - twoPow144;
				Quad upper = Quad.UInt128BitsToQuad(twoPow256bits | (UInt128)(value >> 144)) - twoPow256;

				return lower + upper;
			}
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="double"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator double(UInt256 value)
		{
			const double TwoPow204 = 25711008708143844408671393477458601640355247900524685364822016.0d;
			const double TwoPow256 = 115792089237316195423570985008687907853269984665640564039457584007913129639936.0d;

			const ulong TwoPow204bits = 0x4CB0_0000_0000_0000;
			const ulong TwoPow256bits = 0x4FF0_0000_0000_0000;

			if (value.Upper == 0)
			{
				return value._p1 != 0 ? (double)value.Lower : (double)value._p0;
			}


			double lower = BitConverter.UInt64BitsToDouble(TwoPow204bits | ((ulong)(value.Lower >> 12) >> 12) | ((ulong)(value.Lower) & 0xFFFFFF)) - TwoPow204;
			double upper = BitConverter.UInt64BitsToDouble(TwoPow256bits | (ulong)(value >> 204)) - TwoPow256;

			return lower + upper;
		}
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="float"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator float(UInt256 value) => (float)(double)value;
		/// <summary>
		/// Explicitly converts a <see cref="UInt256" /> value to a <see cref="Half"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator Half(UInt256 value) => (Half)(double)value;
		#endregion

		#region To UInt256
		/// <summary>
		/// Implicitly converts a <see cref="char" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt256(char value) => new UInt256(0, 0, 0, value);
		// Floating
		/// <summary>
		/// Explicitly converts a <see cref="Half" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(Half value) => (UInt256)(double)value;
		/// <summary>
		/// Explicitly converts a <see cref="Half" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(Half value) => checked((UInt256)(double)value);
		/// <summary>
		/// Explicitly converts a <see cref="float" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(float value) => (UInt256)(double)value;
		/// <summary>
		/// Explicitly converts a <see cref="float" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(float value) => checked((UInt256)(double)value);
		/// <summary>
		/// Explicitly converts a <see cref="double" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(double value)
		{
			const double TwoPow256 = 115792089237316195423570985008687907853269984665640564039457584007913129639936.0;

			if (double.IsNegative(value) || double.IsNaN(value))
			{
				return MinValue;
			}
			else if (value >= TwoPow256)
			{
				return MaxValue;
			}

			return ToUInt256(value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="double" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(double value)
		{
			const double TwoPow256 = 115792089237316195423570985008687907853269984665640564039457584007913129639936.0;

			// We need to convert -0.0 to 0 and not throw, so we compare
			// value against 0 rather than checking IsNegative

			if ((value < 0.0) || double.IsNaN(value) || (0.0 < TwoPow256 - value))
			{
				Thrower.IntegerOverflow();
			}
			if (0.0 == TwoPow256 - value)
			{
				return MaxValue;
			}

			return ToUInt256(value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="decimal" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(decimal value) => (UInt256)(double)value;
		/// <summary>
		/// Explicitly converts a <see cref="decimal" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(decimal value) => checked((UInt256)(double)value);
		// Unsigned
		/// <summary>
		/// Implicitly converts a <see cref="byte" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt256(byte value) => new UInt256(0, 0, 0, value);
		/// <summary>
		/// Implicitly converts a <see cref="ushort" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt256(ushort value) => new UInt256(0, 0, 0, value);
		/// <summary>
		/// Implicitly converts a <see cref="uint" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt256(uint value) => new UInt256(0, 0, 0, value);
		/// <summary>
		/// Implicitly converts a <see cref="ulong" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt256(ulong value) => new UInt256(0, 0, 0, value);
		/// <summary>
		/// Implicitly converts a <see cref="UInt128" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt256(UInt128 value) => new UInt256(0, value);
		/// <summary>
		/// Implicitly converts a <see cref="nuint" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static implicit operator UInt256(nuint value) => new UInt256(0, 0, 0, value);
		// Signed
		/// <summary>
		/// Explicitly converts a <see cref="sbyte" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(sbyte value)
		{
			Int128 lower = value;
			return new((UInt128)(lower >> 127), (UInt128)lower);
		}
		/// <summary>
		/// Explicitly converts a <see cref="sbyte" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(sbyte value)
		{
			if (value < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(0, (byte)value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="short" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(short value)
		{
			Int128 lower = value;
			return new((UInt128)(lower >> 127), (UInt128)lower);
		}
		/// <summary>
		/// Explicitly converts a <see cref="short" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(short value)
		{
			if (value < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(0, (UInt128)value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="int" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(int value)
		{
			Int128 lower = value;
			return new((UInt128)(lower >> 127), (UInt128)lower);
		}
		/// <summary>
		/// Explicitly converts a <see cref="int" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(int value)
		{
			if (value < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(0, (UInt128)value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="long" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(long value)
		{
			Int128 lower = value;
			return new((UInt128)(lower >> 127), (UInt128)lower);
		}
		/// <summary>
		/// Explicitly converts a <see cref="long" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(long value)
		{
			if (value < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(0, (UInt128)value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="Int128" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(Int128 value)
		{
			Int128 lower = value;
			return new((UInt128)(lower >> 127), (UInt128)lower);
		}
		/// <summary>
		/// Explicitly converts a <see cref="Int128" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(Int128 value)
		{
			if (value < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(0, (UInt128)value);
		}
		/// <summary>
		/// Explicitly converts a <see cref="nint" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static explicit operator UInt256(nint value)
		{
			Int128 lower = value;
			return new((UInt128)(lower >> 127), (UInt128)lower);
		}
		/// <summary>
		/// Explicitly converts a <see cref="nint" /> value to a <see cref="UInt256"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="UInt256"/>.</exception>
		public static explicit operator checked UInt256(nint value)
		{
			if (value < 0)
			{
				Thrower.IntegerOverflow();
			}
			return new(0, (UInt128)value);
		} 
		#endregion

		private static UInt256 ToUInt256(double value)
		{
			const double TwoPow256 = 115792089237316195423570985008687907853269984665640564039457584007913129639936.0;

			Debug.Assert(value >= 0);
			Debug.Assert(double.IsFinite(value));
			Debug.Assert(0.0 >= TwoPow256 - value);

			// This code is based on `f64_to_u128` from m-ou-se/floatconv
			// Copyright (c) 2020 Mara Bos <m-ou.se@m-ou.se>. All rights reserved.
			//
			// Licensed under the BSD 2 - Clause "Simplified" License
			// See THIRD-PARTY-NOTICES.TXT for the full license text

			if (value >= 1.0)
			{
				// In order to convert from double to uint256 we first need to extract the signficand,
				// including the implicit leading bit, as a full 256-bit significand. We can then adjust
				// this down to the represented integer by right shifting by the unbiased exponent, taking
				// into account the significand is now represented as 256-bits.

				ulong bits = BitConverter.DoubleToUInt64Bits(value);

				var exponent = ((bits >> 52) & 0x7FF) - 1023;
				var matissa = (bits & 0x0F_FFFF_FFFF_FFFF) | 0x10_0000_0000_0000;

				if (exponent <= 52)
				{
					return (UInt256)(matissa >> (int)(52 - exponent));
				}
				else if (exponent >= 256)
				{
					return UInt256.MaxValue;
				}
				else
				{
					return ((UInt256)matissa) << ((int)(exponent - 52));
				}
			}
			else
			{
				return MinValue;
			}
		}

		internal void GetLowerParts(out ulong p1, out ulong p0)
		{
			p1 = _p1;
			p0 = _p0;
		}
		internal void GetUpperParts(out ulong p3, out ulong p2)
		{
			p3 = _p3;
			p2 = _p2;
		}
	}
}
