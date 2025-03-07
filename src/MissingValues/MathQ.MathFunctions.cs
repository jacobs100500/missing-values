﻿using MissingValues.Internals;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MissingValues
{
	internal ref struct Shape
	{
		internal ref Word i;
		internal ref Word64 i2;

		public Shape(ref Quad quad)
		{
			i = ref Unsafe.As<Quad, Word>(ref quad);
			i2 = ref Unsafe.As<Quad, Word64>(ref quad);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Word
		{
#if BIGENDIAN
			internal ushort se;
			internal ushort top;
			internal uint mid;
			internal ulong lo;
#else
			internal ulong lo;
			internal uint mid;
			internal ushort top;
			internal ushort se;
#endif
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct Word64
		{
#if BIGENDIAN
			internal ulong hi;
			internal ulong lo;
#else
			internal ulong lo;
			internal ulong hi;
#endif
		}
	}

	public static partial class MathQ
	{
		internal const int MaxRoundingDigits = 34;

		private static Quad Epsilon => new Quad(0x406F_0000_0000_0000, 0x0000_0000_0000_0000);
		private static Quad INVPIO2 => new Quad(0x3FFE_45F3_06DC_9C88, 0x2A53_F84E_AFA3_EA6A);
		private static Quad PIO2_HI => new Quad(0x3FFF_921F_B544_42D1, 0x8469_898C_C517_01B8);
		private static Quad PIO2_LO => new Quad(0x3F8C_CD12_9024_E088, 0xA67C_C740_20BB_EA64);
		private static Quad M_PI_2 => new Quad(0x3FFF_921F_B544_42D1, 0x8469_898C_C517_01B8); // pi / 2
		private static Quad M_PI_4 => new Quad(0x3FFE_921F_B544_42D1, 0x8469_898C_C517_01B8); // pi / 4
		private static Quad LN2 => new Quad(0x3FFE_62E4_2FEF_A39E, 0xF357_ADEB_B905_E4BD);

		private static Quad RoundLimit => new Quad(0x4073_3426_172C_74D8, 0x22B8_78FE_8000_0000); // 1E35
		internal static ReadOnlySpan<Quad> RoundPower10 => new Quad[MaxRoundingDigits + 1]
		{
			new Quad(0x3FFF_0000_0000_0000, 0x0000_0000_0000_0000), // 1E00
			new Quad(0x4002_4000_0000_0000, 0x0000_0000_0000_0000), // 1E01
			new Quad(0x4005_9000_0000_0000, 0x0000_0000_0000_0000), // 1E02
			new Quad(0x4008_F400_0000_0000, 0x0000_0000_0000_0000), // 1E03
			new Quad(0x400C_3880_0000_0000, 0x0000_0000_0000_0000), // 1E04
			new Quad(0x400F_86A0_0000_0000, 0x0000_0000_0000_0000), // 1E05
			new Quad(0x4012_E848_0000_0000, 0x0000_0000_0000_0000), // 1E06
			new Quad(0x4016_312D_0000_0000, 0x0000_0000_0000_0000), // 1E07
			new Quad(0x4019_7D78_4000_0000, 0x0000_0000_0000_0000), // 1E08
			new Quad(0x401C_DCD6_5000_0000, 0x0000_0000_0000_0000), // 1E09
			new Quad(0x4020_2A05_F200_0000, 0x0000_0000_0000_0000), // 1E10
			new Quad(0x4023_7487_6E80_0000, 0x0000_0000_0000_0000), // 1E11
			new Quad(0x4026_D1A9_4A20_0000, 0x0000_0000_0000_0000), // 1E12
			new Quad(0x402A_2309_CE54_0000, 0x0000_0000_0000_0000), // 1E13
			new Quad(0x402D_6BCC_41E9_0000, 0x0000_0000_0000_0000), // 1E14
			new Quad(0x4030_C6BF_5263_4000, 0x0000_0000_0000_0000), // 1E15
			new Quad(0x4034_1C37_937E_0800, 0x0000_0000_0000_0000), // 1E16
			new Quad(0x4037_6345_785D_8A00, 0x0000_0000_0000_0000), // 1E17
			new Quad(0x403A_BC16_D674_EC80, 0x0000_0000_0000_0000), // 1E18
			new Quad(0x403E_158E_4609_13D0, 0x0000_0000_0000_0000), // 1E19
			new Quad(0x4041_5AF1_D78B_58C4, 0x0000_0000_0000_0000), // 1E20
			new Quad(0x4044_B1AE_4D6E_2EF5, 0x0000_0000_0000_0000), // 1E21
			new Quad(0x4048_0F0C_F064_DD59, 0x2000_0000_0000_0000), // 1E22
			new Quad(0x404B_52D0_2C7E_14AF, 0x6800_0000_0000_0000), // 1E23
			new Quad(0x404E_A784_379D_99DB, 0x4200_0000_0000_0000), // 1E24
			new Quad(0x4052_08B2_A2C2_8029, 0x0940_0000_0000_0000), // 1E25
			new Quad(0x4055_4ADF_4B73_2033, 0x4B90_0000_0000_0000), // 1E26
			new Quad(0x4058_9D97_1E4F_E840, 0x1E74_0000_0000_0000), // 1E27
			new Quad(0x405C_027E_72F1_F128, 0x1308_8000_0000_0000), // 1E28
			new Quad(0x405F_431E_0FAE_6D72, 0x17CA_A000_0000_0000), // 1E29
			new Quad(0x4062_93E5_939A_08CE, 0x9DBD_4800_0000_0000), // 1E30
			new Quad(0x4065_F8DE_F880_8B02, 0x452C_9A00_0000_0000), // 1E31
			new Quad(0x4069_3B8B_5B50_56E1, 0x6B3B_E040_0000_0000), // 1E32
			new Quad(0x406C_8A6E_3224_6C99, 0xC60A_D850_0000_0000), // 1E33
			new Quad(0x406F_ED09_BEAD_87C0, 0x378D_8E64_0000_0000), // 1E34
		};

		// Domain [-0.7854, 0.7854], range ~[-1.80e-37, 1.79e-37]:
		// |cos(newBase) - c(newBase))| < 2**-122.0
		private static Quad C1 => new Quad(0x3FFA_5555_5555_5555, 0x5555_5555_5555_5548);
		private static Quad C2 => new Quad(0xBFF5_6C16_C16C_16C1, 0x6C16_C16C_16BF_5C98);
		private static Quad C3 => new Quad(0x3FEF_A01A_01A0_1A01, 0xA01A_019F_FFC4_B13D);
		private static Quad C4 => new Quad(0xBFE9_27E4_FB77_89F5, 0xC72E_EF94_869C_AC2A);
		private static Quad C5 => new Quad(0x3FE2_1EED_8EFF_8D89, 0x7B51_B5F6_2EA9_599A);
		private static Quad C6 => new Quad(0xBFDA_9397_4A8C_07C9, 0xC2A3_8FC4_4BBC_8DF5);
		private static Quad C7 => new Quad(0x3FD2_AE7F_3E73_3B48, 0xDDA7_3725_A8CB_76C2);
		private static Quad C8 => new Quad(0xBFCA_6827_863B_100E, 0xC1D2_05BD_6344_4584);
		private static Quad C9 => new Quad(0x3FC1_E542_B8A1_08C0, 0x71EB_27E7_68BA_79E3);
		private static Quad C10 => new Quad(0xBFB9_0CE2_0CD8_68A2, 0x04B8_FF44_E6BF_56E0);
		private static Quad C11 => new Quad(0x3FAF_EF81_27D7_65B0, 0x90B7_B2A6_9D9B_4DA3);

		// Domain [-0.7854, 0.7854], range ~[-1.53e-37, 1.659e-37]
		// |sin(newBase)/newBase - s(newBase)| < 2**-122.1
		private static Quad S1 => new Quad(0xBFFC_5555_5555_5555, 0x5555_5555_5555_5555);
		private static Quad S2 => new Quad(0x3FF8_1111_1111_1111, 0x1111_1111_1111_107F);
		private static Quad S3 => new Quad(0xBFF2_A01A_01A0_1A01, 0xA01A_01A0_19F8_F785);
		private static Quad S4 => new Quad(0x3FEC_71DE_3A55_6C73, 0x38FA_AC1C_5571_67FE);
		private static Quad S5 => new Quad(0xBFE5_AE64_567F_544E, 0x38FE_7342_6974_AE93);
		private static Quad S6 => new Quad(0x3FDE_6124_613A_86D0, 0x97C5_C00C_FA3D_6509);
		private static Quad S7 => new Quad(0xBFD6_AE7F_3E73_3B81, 0xDC97_972A_DED6_8D8D);
		private static Quad S8 => new Quad(0x3FCE_952C_7703_0A96, 0x9D8A_B423_F5C4_7870);
		private static Quad S9 => new Quad(0xBFC6_2F49_B467_96FE, 0x3000_0000_0000_0000);
		private static Quad S10 => new Quad(0x3FBD_71B8_EE20_94BA, 0xE000_0000_0000_0000);
		private static Quad S11 => new Quad(0xBFB4_7619_0E26_27FC, 0xD000_0000_0000_0000);
		private static Quad S12 => new Quad(0x3FAB_3D19_FFD7_AD8B, 0xF000_0000_0000_0000);
		// Domain [-0.67434, 0.67434], range ~[-3.37e-36, 1.982e-37]
		// |tan(newBase)/newBase - t(newBase)| < 2**-117.8 (XXX should be ~1e-37)
		private static Quad T3 => new Quad(0x3FFD_5555_5555_5555, 0x5555_5555_5555_5555);
		private static Quad T5 => new Quad(0x3FFB_1111_1111_1111, 0x1111_1111_1111_1111);
		private static Quad T7 => new Quad(0x3FF9_4AFD_6A05_2BF5, 0xA814_AFD6_A052_BF5B);
		private static Quad T9 => new Quad(0x3FF8_BACF_914C_1BAC, 0xF914_C1BA_CF91_4C1C);
		private static Quad T11 => new Quad(0x3FF8_3991_C2C1_87F6, 0x3371_E9F3_C04E_6471);
		private static Quad T13 => new Quad(0x3FF7_C46E_2EDF_04B7, 0x7CF3_29D6_48F7_37F3);
		private static Quad T15 => new Quad(0x3FF7_50FF_D3D7_CBD5, 0xE03D_BA3D_CD6C_A9DE);
		private static Quad T17 => new Quad(0x3FF7_035D_CFDB_8799, 0x4DF9_3F49_6D43_A78E);
		private static Quad T19 => new Quad(0x3FF6_3CED_5147_D527, 0xDE68_E31D_1D4D_A384);
		private static Quad T21 => new Quad(0x3FF5_69F0_F52C_180C, 0xD4F8_26F2_B404_E630);
		private static Quad T23 => new Quad(0x3FF4_9D3B_1169_5AC7, 0x1678_4F31_194D_1DA6);
		private static Quad T25 => new Quad(0x3FF3_CDE3_E0F7_FB55, 0x386E_CE7F_1EE1_C803);
		private static Quad T27 => new Quad(0x3FF3_0100_7209_8FB6, 0x724C_7800_92C5_9B12);
		private static Quad T29 => new Quad(0x3FF2_20DF_545B_CF0B, 0x653C_F18A_0CF1_3671);
		private static Quad T31 => new Quad(0x3FF1_4335_BB9F_BC81, 0x8BCF_005A_4BF6_DB2C);
		private static Quad T33 => new Quad(0x3FF0_6966_313F_170F, 0xED67_CE16_ADD6_8D4C);
		private static Quad T35 => new Quad(0x3FEF_920B_2709_36E0, 0x6EF5_8910_3109_E713);
		private static Quad T37 => new Quad(0x3FEE_C4C4_612C_6A34, 0x2B75_272F_80AC_827B);
		private static Quad Pio4 => new Quad(0x3FFE_921F_B544_42D1, 0x8469_898C_C517_01B8);
		private static Quad PIO4LO => new Quad(0x3F8C_CD12_90C7_B259, 0x07DD_492F_6840_C751);
		private static Quad T39 => new Quad(0x3FE5_E8A7_5929_7793, 0x7F54_CCE1_AFCF_5393);
		private static Quad T41 => new Quad(0x3FE4_9BAA_1B12_2321, 0x8F8C_A1EF_55EF_5447);
		private static Quad T43 => new Quad(0x3FE3_0738_5DFB_2452, 0x9040_8081_0F2D_A186);
		private static Quad T45 => new Quad(0x3FE2_DC6C_702A_0526, 0x20B3_B94C_D647_048A);
		private static Quad T47 => new Quad(0xBFE1_9ECE_F356_9EBB, 0x5EDD_03A8_74BE_875B);
		private static Quad T49 => new Quad(0x3FE2_94C0_668D_A786, 0x9F57_234D_6237_8C55);
		private static Quad T51 => new Quad(0xBFE2_2E76_3B88_4526, 0x808D_F7F5_A784_CC40);
		private static Quad T53 => new Quad(0x3FE1_A92F_C98C_2955, 0x3DA4_66BA_7143_E38D);
		private static Quad T55 => new Quad(0xBFE0_5110_6CBC_779A, 0x8FC0_7E96_AD48_C11E);
		private static Quad T57 => new Quad(0x3FDE_47ED_BDBA_6F43, 0xA141_91C3_09F7_7315);


		/*
		 * Most functions, unless specified, are based on musl libc
		 * source: https://git.musl-libc.org/cgit/musl
		 * 
		 * ====================================================
		 * Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
		 *
		 * Developed at SunPro, a Sun Microsystems, Inc. business.
		 * Permission to use, copy, modify, and distribute this
		 * software is freely granted, provided that this notice
		 * is preserved.
		 * ====================================================
		 */


		/// <summary>
		/// Returns the absolute value of a quadruple-precision floating-point number.
		/// </summary>
		/// <param name="x">A number that is greater than or equal to <seealso cref="Quad.MinValue"/>, but less than or equal to <seealso cref="Quad.MaxValue"/>.</param>
		/// <returns>A quadruple-precision floating-point number, x, such that 0 ≤ x ≤ <seealso cref="Quad.MaxValue"/>.</returns>
		public static Quad Abs(Quad x)
		{
			return Quad.UInt128BitsToQuad(Quad.QuadToUInt128Bits(x) & Quad.InvertedSignMask);
		}
		/// <summary>
		/// Returns the angle whose cosine is the specified number.
		/// </summary>
		/// <param name="x">A number representing a cosine, where <paramref name="x"/> must be greater than or equal to -1, but less than or equal to 1.</param>
		/// <returns>
		/// An angle, θ, measured in radians, such that 0 ≤ θ ≤ π.
		/// </returns>
		public static Quad Acos(Quad x)
		{
			Quad y;

			if (x == Quad.Zero)
			{
				return M_PI_2;
			}
			if (x >= Quad.One)
			{
				if (x == Quad.One)
				{
					return Quad.Zero;
				}
				return Quad.NaN;
			}
			if (x <= Quad.NegativeOne)
			{
				if (x == Quad.NegativeOne)
				{
					return Quad.Pi;
				}
				return Quad.NaN;
			}

			y = Atan(Sqrt(Quad.One - (x * x)) / x);

			if (x > Quad.Zero)
			{
				return y;
			}

			return y + Quad.Pi;
		}
		/// <summary>
		/// Returns the angle whose hyperbolic cosine is the specified number.
		/// </summary>
		/// <param name="x">A number representing a hyperbolic cosine, where <paramref name="x"/> must be greater than or equal to 1, but less than or equal to <seealso cref="Quad.PositiveInfinity"/>.</param>
		/// <returns>An angle, θ, measured in radians, such that 0 ≤ θ ≤ ∞.</returns>
		public static Quad Acosh(Quad x)
		{
			Quad t;
			var exponent = x.BiasedExponent;

			if (exponent < 0x3FFF || (exponent & 0x8000) != 0)
			{
				return Quad.NaN;
			}
			else if (exponent >= 0x401D) // y > 2^30
			{
				if (exponent >= 0x7FFF)
				{
					return x;
				}

				else
				{
					return MathQ.Log(x) + LN2;
				}
			}
			else if (x == Quad.One)
			{
				return Quad.Zero;
			}
			else if (exponent > 0x4000) // 2^28 > y > 2
			{
				t = x * x;
				return MathQ.Log(Quad.Two * x - Quad.One / (x + MathQ.Sqrt(t - Quad.One)));
			}
			else // 1 < y < 2
			{
				t = (x - Quad.One);
				return Quad.LogP1(t + Sqrt(Quad.Two * t + t * t));
			}
		}
		/// <summary>
		/// Returns the angle whose sine is the specified number.
		/// </summary>
		/// <param name="x">A number representing a sine, where <paramref name="x"/> must be greater than or equal to -1, but less than or equal to 1.</param>
		/// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
		public static Quad Asin(Quad x)
		{
			Quad z, r, s;
			ushort exponent = x.BiasedExponent;
			bool sign = Quad.IsNegative(x);

			if (exponent >= 0x3FFF) // |x| >= 1 or nan
			{
				// asin(+-1)=+-pi/2 with inexact
				if (x == Quad.One || x == Quad.NegativeOne)
				{
					return x * PIO2_HI + new Quad(0x3F87_0000_0000_0000, 0x0000_0000_0000_0000);
				}

				return Quad.NaN;
			}
			if (exponent < 0x3FFF - 1) // |x| < 0.5
			{
				if (exponent < 0x3FFF - (Quad.MantissaDigits + 1) / 2)
				{
					return x;
				}

				return x + x * Constants.Asin.R(x * x);
			}
			// 1 > |x| >= 0.5
			z = (Quad.One - Quad.Abs(x)) * Quad.HalfOne;
			s = Sqrt(z);
			r = Constants.Asin.R(z);
			if (((x._upper >> 32) & 0xFFFF) >= 0xEE00) // Close to 1
			{
				x = PIO2_HI - (Quad.Two * (s + s * r) - PIO2_LO);
			}
			else
			{
				Quad f, c;
				f = new Quad(s._upper, 0x0000_0000_0000_0000);
				c = (z - f * f) / (s + f);
				x = Quad.HalfOne * PIO2_HI - (Quad.Two * s * r - (PIO2_LO - Quad.Two * c) - (Quad.HalfOne * PIO2_HI - Quad.Two * f));
			}

			return sign ? -x : x;
		}
		/// <summary>
		/// Returns the angle whose hyperbolic sine is the specified number.
		/// </summary>
		/// <param name="x">A number representing a hyperbolic sine, where <paramref name="x"/> must be greater than or equal to <see cref="Quad.NegativeInfinity"/>, but less than or equal to <see cref="Quad.PositiveInfinity"/>.</param>
		/// <returns>An angle, θ, measured in radians.</returns>
		public static Quad Asinh(Quad x)
		{
			// asinh(x) = sign(x)*log(|x|+sqrt(x*x+1)) ~= x - x^3/6 + o(x^5)

			var exponent = x.BiasedExponent;
			var sign = Quad.IsNegative(x);

			// |x|
			x = Quad.Abs(x);

			if (exponent >= 0x401F)
			{
				// |x| >= 0x1p32 or inf or nan 
				x = Log(x) + new Quad(0x3FFE_62E4_2FEF_A39E, 0xF357_93C7_6730_07E6); // Log(x) + Ln(2)
			}
			else if (exponent >= 0x4000)
			{
				// |x| >= 2
				x = Log(Quad.Two * x + Quad.One / (Sqrt(x * x + Quad.One) + x));
			}
			else if (exponent >= 0x3FDF)
			{
				// |x| >= 0x1p-32
				x = Quad.LogP1(x + x * x / (Sqrt(x * x + Quad.One) + Quad.One));
			}

			return sign ? -x : x;
		}
		/// <summary>
		/// Returns the angle whose tangent is the specified number.
		/// </summary>
		/// <param name="x">A number representing a tangent.</param>
		/// <returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2.</returns>
		public static Quad Atan(Quad x)
		{
			Quad w, s1, s2, z;
			int id;
			var exponent = x.BiasedExponent;
			var sign = Quad.IsNegative(x);

			if (exponent >= 0x3FFF + (Quad.MantissaDigits + 1))
			{
				// if |x| is large, atan(x)~=pi/2
				if (Quad.IsNaN(x))
				{
					return x;
				}
				return sign ? -Constants.Atan.AtanHi[3] : Constants.Atan.AtanHi[3];
			}

			// Extract the exponent and the first few bits of the mantissa.
			uint expman = ((uint)(exponent << 8) | ((byte)(x._upper >> 40)));
			if (expman < ((0x3FFF - 2) << 8) + 0xC0) // |x| < 0.4375
			{
				if (exponent < 0x3FFF - (Quad.MantissaDigits + 1) / 2)
				{
					// if |x| is small, atan(x)~=x
					if (Quad.IsSubnormal(x))
					{
						x = Quad.NegativeInfinity;
					}
					return x;
				}

				id = -1;
			}
			else
			{
				x = Quad.Abs(x);

				if (expman < ((0x3fff << 8) + 0x30))
				{
					// |x| < 1.1875
					if (expman < ((0x3fff - 1) << 8) + 0x60)
					{
						// 7/16 <= |x| < 11/16 
						id = 0;
						x = (Quad.Two * x - Quad.One) / (Quad.Two + x);
					}
					else
					{
						id = 1;
						x = (x - Quad.One) / (x + Quad.One);
					}
				}
				else
				{
					if (expman < ((0x3fff + 1) << 8) + 0x38)
					{
						id = 2;
						Quad oneHalf = new Quad(0x3FFF_8000_0000_0000, 0x0000_0000_0000_0000);
						x = (x - oneHalf) / (Quad.One + oneHalf * x);
					}
					else
					{
						id = 3;
						x = Quad.NegativeOne / x;
					}
				}
			}

			// end of argument reduction
			z = x * x;
			w = z * z;
			// break sum aT[i]z**(i+1) into odd and even poly
			s1 = z * Constants.Atan.Even(w);
			s2 = w * Constants.Atan.Odd(w);
			if (id < 0)
				return x - x * (s1 + s2);
			z = Constants.Atan.AtanHi[id] - ((x * (s1 + s2) - Constants.Atan.AtanLo[id]) - x);
			return sign ? -z : z;
		}
		/// <summary>
		/// Returns the angle whose tangent is the quotient of two specified numbers.
		/// </summary>
		/// <param name="y">The y coordinate of a point.</param>
		/// <param name="x">The x coordinate of a point.</param>
		/// <returns>An angle, θ, measured in radians, such that -π ≤ θ ≤ π, and tan(θ) = <paramref name="y"/> / <paramref name="x"/>, where (<paramref name="x"/>, <paramref name="y"/>) is a point in the Cartesian plane.</returns>
		public static Quad Atan2(Quad y, Quad x)
		{
			Quad z;
			int m, ex, ey;

			if (Quad.IsNaN(x))
			{
				return x;
			}
			if (Quad.IsNaN(y))
			{
				return y;
			}

			ex = x.BiasedExponent;
			ey = y.BiasedExponent;
			m = (int)(2 * (x._upper >> 63) | (y._upper >> 63));

			if (y == Quad.Zero)
			{
				switch (m)
				{
					case 0:
					case 1:
						return y;
					case 2:
						return Quad.Two * PIO2_HI;
					case 3:
						return -Quad.Two * PIO2_HI;
					default:
						break;
				}
			}
			if (x == Quad.Zero)
			{
				return ((m & 1) != 0) ? -PIO2_HI : PIO2_HI;
			}
			if (ex == 0x7FFF)
			{
				if (ey == 0x7FFF)
				{
					Quad oneHalf = new Quad(0x3FFF_8000_0000_0000, 0x0000_0000_0000_0000);
					switch (m)
					{
						case 0: // atan(+INF,+INF)
							return PIO2_HI / Quad.Two;
						case 1: // atan(-INF,+INF)
							return -PIO2_HI / Quad.Two;
						case 2: // atan(+INF,-INF)
							return oneHalf * PIO2_HI;
						case 3: // atan(-INF,-INF)
							return -oneHalf * PIO2_HI;
						default:
							break;
					}
				}
				else
				{
					switch (m)
					{
						case 0: // atan(+...,+INF)
							return Quad.Zero;
						case 1: // atan(-...,+INF)
							return Quad.NegativeZero;
						case 2: // atan(+...,-INF)
							return Quad.Two * PIO2_HI;
						case 3: // atan(-...,-INF)
							return -Quad.Two * PIO2_HI;
						default:
							break;
					}
				}
			}
			if (ex + 120 < ey || ey == 0x7FFF)
			{
				return ((m & 1) != 0) ? -PIO2_HI : PIO2_HI;
			}
			// z = atan(|y/x|) without spurious underflow
			if (((m & 2) != 0) && ey + 120 < ex)
			{
				z = Quad.Zero;
			}
			else
			{
				z = Atan(Abs(y / x));
			}

			switch (m)
			{
				case 0: // atan(+,+)
					return z;
				case 1: // atan(-,+)
					return -z;
				case 2: // atan(+,-)
					return Quad.Two * PIO2_HI - (z - Quad.Two * PIO2_LO);
				default: // atan(-,-)
					return (z - Quad.Two * PIO2_LO) - Quad.Two * PIO2_HI;
			}
		}
		/// <summary>
		/// Returns the angle whose hyperbolic tangent is the specified number.
		/// </summary>
		/// <param name="x">A number representing a hyperbolic tangent, where <paramref name="x"/> must be greater than or equal to -1, but less than or equal to 1.</param>
		/// <returns>An angle, θ, measured in radians.</returns>
		public static Quad Atanh(Quad x)
		{
			var exponent = x.BiasedExponent;
			var sign = Quad.IsNegative(x);

			x = Abs(x);

			if (exponent < 0x3FF - 1)
			{
				if (exponent >= 0x3FF - 113 / 2)
				{
					x = Quad.HalfOne * Quad.LogP1(Quad.Two * x + Quad.Two * x * x / (Quad.One - x));
				}
			}
			else
			{
				x = Quad.HalfOne * Quad.LogP1(Quad.Two * (x / (Quad.One - x)));
			}

			return sign ? -x : x;
		}
		/// <summary>
		/// Returns the largest value that compares less than a specified value.
		/// </summary>
		/// <param name="x">The value to decrement.</param>
		/// <returns>The largest value that compares less than <paramref name="x"/>.</returns>
		public static Quad BitDecrement(Quad x)
		{
			UInt128 bits = Quad.QuadToUInt128Bits(x);

			if ((bits & Quad.PositiveInfinityBits) >= Quad.PositiveInfinityBits)
			{
				// NaN returns NaN
				// -Infinity returns -Infinity
				// +Infinity returns MaxValue
				return (bits == Quad.PositiveInfinityBits) ? Quad.MaxValue : x;
			}

			if (bits == Quad.PositiveZeroBits)
			{
				// +0.0 returns -Epsilon
				return -Quad.Epsilon;
			}

			// Negative values need to be incremented
			// Positive values need to be decremented

			bits += unchecked((UInt128)(((Int128)bits < Int128.Zero) ? Int128.One : Int128.NegativeOne));
			return Quad.UInt128BitsToQuad(bits);
		}
		/// <summary>
		/// Returns the smallest value that compares greater than a specified value.
		/// </summary>
		/// <param name="x">The value to increment.</param>
		/// <returns>The smallest value that compares greater than <paramref name="x"/>.</returns>
		public static Quad BitIncrement(Quad x)
		{
			UInt128 bits = Quad.QuadToUInt128Bits(x);

			if ((bits & Quad.PositiveInfinityBits) >= Quad.PositiveInfinityBits)
			{
				// NaN returns NaN
				// -Infinity returns MinValue
				// +Infinity returns +Infinity
				return (bits == Quad.NegativeInfinityBits) ? Quad.MinValue : x;
			}

			if (bits == Quad.NegativeZeroBits)
			{
				// -0.0 returns Epsilon
				return Quad.Epsilon;
			}

			// Negative values need to be decremented
			// Positive values need to be incremented

			bits += unchecked((UInt128)(((Int128)bits < Int128.Zero) ? Int128.NegativeOne : Int128.One));
			return Quad.UInt128BitsToQuad(bits);
		}
		/// <summary>
		/// Returns the cube root of a specified number.
		/// </summary>
		/// <param name="x">The number whose cube root is to be found.</param>
		/// <returns>The cube root of <paramref name="x"/>.</returns>
		public static Quad Cbrt(Quad x)
		{
			const uint B1 = 709958130; // B1 = (127 - 127.0/3 - 0.03306235651) * 2^23

			Quad r, s, t, w;
			double dr, dt, dx;
			float ft;
			int e = x.BiasedExponent;
			int sign = Quad.IsNegative(x) ? 1 << 15 : 0;
			Quad u = x;

			/*
			 * If x = +-Inf, then cbrt(x) = +-Inf.
			 * If x = NaN, then cbrt(x) = NaN.
			 */

			if (e == 0x7FFF)
			{
				return x;
			}
			if (e == 0)
			{
				// Adjust subnormal numbers
				u *= new Quad(0x4077_0000_0000_0000, 0x0000_0000_0000_0000); // u *= 0x1p120
				e = u.BiasedExponent;
				// If x = +-0, then Cbrt(x) = +-0.
				if (e == 0)
				{
					return x;
				}
				e -= 120;
			}
			e -= 0x3FFF;
			x = new Quad(false, 0x3FFF, u.TrailingSignificand);

			switch (e % 3)
			{
				case 1:
				case -2:
					x *= Quad.Two;
					e--;
					break;
				case 2:
				case -1:
					x *= new Quad(0x4001_0000_0000_0000, 0x0000_0000_0000_0000); // x *= 4
					e -= 2;
					break;
			}

			Quad v = Quad.UInt128BitsToQuad(new UInt128(((ulong)(sign | (0x3FFF + e / 3)) << 48), 0));

			/*
			 * The following is the guts of s_cbrtf, with the handling of
			 * special values removed and extra care for accuracy not taken,
			 * but with most of the extra accuracy not discarded.
			 */

			// ~5-bit estimate:
			ft = BitConverter.UInt32BitsToSingle((BitConverter.SingleToUInt32Bits((float)x) & 0x7FFF_FFFF) / 3 + B1);

			// ~16-bit estimate:
			dx = (double)x;
			dt = ft;
			dr = dt * dt * dt;
			dt = dt * (dx + dx + dr) / (dx + dr + dr);

			// ~47-bit estimate:
			dr = dt * dt * dt;
			dt = dt * (dx + dx + dr) / (dx + dr + dr);

			/*
			 * Round dt away from zero to 47 bits.  Since we don't trust the 47,
			 * add 2 47-bit ulps instead of 1 to round up.  Rounding is slow and
			 * might be avoidable in this case, since on most machines dt will
			 * have been evaluated in 53-bit precision and the technical reasons
			 * for rounding up might not apply to either case in cbrtl() since
			 * dt is much more accurate than needed.
			 */

			// t = dt + 0x2.0p-46 + 0x1.0p60L - 0x1.0p60
			t = dt + new Quad(0x3FD2_0000_0000_0000, 0x0000_0000_0000_0000) + new Quad(0x403B_0000_0000_0000, 0x0000_0000_0000_0000) - new Quad(0x403B_0000_0000_0000, 0x0000_0000_0000_0000);

			/*
			 * Final step Newton iteration to 113 bits with
			 * error < 0.667 ulps
			 */

			s = t * t;              // t*t is exact
			r = x / s;              // error <= 0.5 ulps; |r| < |t|
			w = t + t;              // t+t is exact
			r = (r - t) / (w + r);  // r-t is exact; w+r ~= 3*t
			t = t + t * r;          // error <= 0.5 + 0.5/3 + epsilon

			return t * v;
		}
		/// <summary>
		/// Returns the smallest integral value that is greater than or equal to the specified quadruple-precision floating-point number.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point</param>
		/// <returns>The smallest integral value that is greater than or equal to <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NegativeInfinity"/>, or <see cref="Quad.PositiveInfinity"/>, that value is returned. Note that this method returns a <see cref="Quad"/> instead of an integral type.</returns>
		public static Quad Ceiling(Quad x)
		{
			var exponent = x.BiasedExponent;
			bool sign = Quad.IsNegative(x);
			Quad y;

			if (exponent >= 0x3FFF + Quad.MantissaDigits - 1 || x == Quad.Zero)
			{
				return x;
			}
			// newBase = int(x) - x, where int(x) is an integer neighbor of x
			Quad toint = Epsilon;
			if (sign)
			{
				y = x - toint + toint - x;
			}
			else
			{
				y = x + toint - toint - x;
			}
			// special case because of non-nearest rounding modes
			if (exponent <= 0x3FFF - 1)
			{
				return sign ? Quad.NegativeZero : Quad.One;
			}
			if (y < Quad.Zero)
			{
				return x + y + Quad.One;
			}
			return x + y;
		}
		/// <summary>
		/// Returns <paramref name="value"/> clamped to the inclusive range of <paramref name="min"/> and <paramref name="max"/>.
		/// </summary>
		/// <param name="value">The value to be clamped</param>
		/// <param name="min">The lower bound of the result</param>
		/// <param name="max">The upper bound of the result</param>
		/// <returns><paramref name="value"/> if <paramref name="min"/> ≤ <paramref name="value"/> ≤ <paramref name="max"/>.</returns>
		public static Quad Clamp(Quad value, Quad min, Quad max)
		{
			if (min > max)
			{
				Thrower.MinMaxError(min, max);
			}

			if (value < min)
			{
				return min;
			}
			else if (value > max)
			{
				return max;
			}

			return value;
		}
		/// <summary>
		/// Returns a value with the magnitude of <paramref name="x"/> and the sign of <paramref name="y"/>.
		/// </summary>
		/// <param name="x">A number whose magnitude is used in the result.</param>
		/// <param name="y">A number whose sign is the used in the result.</param>
		/// <returns>A value with the magnitude of <paramref name="x"/> and the sign of <paramref name="y"/>.</returns>
		public static Quad CopySign(Quad x, Quad y)
		{
			// This method is required to work for all inputs,
			// including NaN, so we operate on the raw bits.
			UInt128 xbits = Quad.QuadToUInt128Bits(x);
			UInt128 ybits = Quad.QuadToUInt128Bits(y);

			// Remove the sign from y, and remove everything but the sign from x
			xbits &= Quad.InvertedSignMask;
			ybits &= Quad.SignMask;

			// Simply OR them to get the correct sign
			return Quad.UInt128BitsToQuad(xbits | ybits);
		}

		private static int __rem_pio2l(Quad x, Span<Quad> y)
		{
			/* origin: FreeBSD /usr/src/lib/msun/ld128/e_rem_pio2.c */
			const int ROUND1 = 51;
			const int ROUND2 = 119;
			const int NX = 5;
			const int NY = 3;

			Quad u = x, z, w, t, r, fn;
			Span<double> tx = stackalloc double[NX];
			Span<double> ty = stackalloc double[NY];
			long n;
			int ex, i;

			ex = x.BiasedExponent;

			if (Abs(x) < new Quad(0x402C_921F_0000_0000, 0x0000_0000_0000_0000))
			{ // |x| ~< 2^45*(pi/2), medium size
				fn = x * INVPIO2 + Epsilon - Epsilon; /* rint(x/(pi/2)) */
				n = ((uint)(long)fn & 0x7FFF_FFFF);
				r = x - fn * Constants.RemPio.PIO2_1;
				w = fn * Constants.RemPio.PIO2_1T; // 1st round good to 180 bit
												   // Matters with directed rounding
				Quad temp = r - w;
				if (temp < -Pio4)
				{
					n--;
					fn--;
					r = x - fn * Constants.RemPio.PIO2_1;
					w = fn * Constants.RemPio.PIO2_1T;
				}
				else if (temp > Pio4)
				{
					n++;
					fn++;
					r = x - fn * Constants.RemPio.PIO2_1;
					w = fn * Constants.RemPio.PIO2_1T;
				}

				y[0] = r - w;
				u = y[0];
				int ey = u.BiasedExponent;
				if (ex - ey > ROUND1) // 2nd iteration needed, good to 248
				{
					t = r;
					w = fn * Constants.RemPio.PIO2_2;
					r = t - w;
					w = fn * Constants.RemPio.PIO2_2T - ((t - r) - w);
					y[0] = r - w;
					u = y[0];
					ey = u.BiasedExponent;
					if (ex - ey > ROUND2) // 3rd iteration need, 316 bits acc
					{
						t = r; // will cover all possible cases
						w = fn * Constants.RemPio.PIO2_3;
						r = t - w;
						w = fn * Constants.RemPio.PIO2_3T - ((t - r) - w);
						y[0] = r - w;
					}
				}
				y[1] = (r - y[0]) - w;
				return (int)n;
			}

			// All other (large) arguments
			if (ex == 0x7FFF)
			{
				y[0] = y[1] = x;
				return 0;
			}
			// set z = scalbn(|x|,-ilogb(x)+23)
			z = new Quad(false, 0x3FFF + 23, x.TrailingSignificand);

			for (i = 0; i < NX - 1; i++)
			{
				tx[i] = (int)z;
				z = (z - tx[i]) * new Quad(0x4017_0000_0000_0000, 0x0000_0000_0000_0000);
			}
			tx[i] = (double)z;
			while (tx[i] == 0)
			{
				i--;
			}
			n = __rem_pio2_large(tx, ty, ex - 0x3FFF - 23, i + 1, NY);
			t = (Quad)ty[2] + ty[1];
			r = t + ty[0];
			w = ty[0] - (r - t);
			if (Quad.IsNegative(u))
			{
				y[0] = -r;
				y[1] = -w;
				return -(int)n;
			}
			y[0] = r;
			y[1] = w;
			return (int)n;

			static int __rem_pio2_large(Span<double> x, Span<double> y, int e0, int nx, int prec)
			{
				int jz, jx, jv, jp, jk, carry, n, i, j, k, m, q0, ih;
				Span<int> iq = stackalloc int[20];
				double z, fw;
				Span<double> f = stackalloc double[20];
				Span<double> fq = stackalloc double[20];
				Span<double> q = stackalloc double[20];

				/* initialize jk*/
				jk = Constants.RemPio.INIT_JK[prec];
				jp = jk;

				/* determine jx,jv,q0, note that 3>q0 */
				jx = nx - 1;
				jv = (e0 - 3) / 24; if (jv < 0) jv = 0;
				q0 = e0 - 24 * (jv + 1);

				/* set up f[0] to f[jx+jk] where f[jx+jk] = ipio2[jv+jk] */
				j = jv - jx; m = jx + jk;
				for (i = 0; i <= m; i++, j++)
					f[i] = j < 0 ? 0.0 : Constants.RemPio.IPIO2[j];

				/* compute q[0],q[1],...q[jk] */
				for (i = 0; i <= jk; i++)
				{
					for (j = 0, fw = 0.0; j <= jx; j++)
						fw += x[j] * f[jx + i - j];
					q[i] = fw;
				}

				jz = jk;
			recompute:
				/* distill q[] into iq[] reversingly */
				for (i = 0, j = jz, z = q[jz]; j > 0; i++, j--)
				{
					fw = (int)(5.9604644775390625E-8 * z);
					iq[i] = (int)(z - 1.6777216E7 * fw);
					z = q[j - 1] + fw;
				}
				// compute n
				z = Math.ScaleB(z, q0);     // actual value of z
				z -= 8.0 * Math.Floor(z * 0.125);   // trim off integer >= 8 
				n = (int)z;
				z -= n;
				ih = 0;

				if (q0 > 0)
				{  /* need iq[jz-1] to determine n */
					i = iq[jz - 1] >> (24 - q0); n += i;
					iq[jz - 1] -= i << (24 - q0);
					ih = iq[jz - 1] >> (23 - q0);
				}
				else if (q0 == 0)
					ih = iq[jz - 1] >> 23;
				else if (z >= 0.5)
					ih = 2;

				if (ih > 0)
				{  /* q > 0.5 */
					n += 1; carry = 0;
					for (i = 0; i < jz; i++)
					{  /* compute 1-q */
						j = iq[i];
						if (carry == 0)
						{
							if (j != 0)
							{
								carry = 1;
								iq[i] = 0x1000000 - j;
							}
						}
						else
							iq[i] = 0xffffff - j;
					}
					if (q0 > 0)
					{  /* rare case: chance is 1 in 12 */
						switch (q0)
						{
							case 1:
								iq[jz - 1] &= 0x7fffff; break;
							case 2:
								iq[jz - 1] &= 0x3fffff; break;
						}
					}
					if (ih == 2)
					{
						z = 1.0 - z;
						if (carry != 0)
							z -= Math.ScaleB(1.0, q0);
					}
				}

				/* check if recomputation is needed */
				if (z == 0.0)
				{
					j = 0;
					for (i = jz - 1; i >= jk; i--) j |= iq[i];
					if (j == 0)
					{  /* need recomputation */
						for (k = 1; iq[jk - k] == 0; k++) ;  /* k = no. of terms needed */

						for (i = jz + 1; i <= jz + k; i++)
						{  /* add q[jz+1] to q[jz+k] */
							f[jx + i] = Constants.RemPio.IPIO2[jv + i];
							for (j = 0, fw = 0.0; j <= jx; j++)
								fw += x[j] * f[jx + i - j];
							q[i] = fw;
						}
						jz += k;
						goto recompute;
					}
				}

				/* chop off zero terms */
				if (z == 0.0)
				{
					jz -= 1;
					q0 -= 24;
					while (iq[jz] == 0)
					{
						jz--;
						q0 -= 24;
					}
				}
				else
				{ /* break z into 24-bit if necessary */
					z = Math.ScaleB(z, -q0);
					if (z >= 1.6777216E7)
					{
						fw = (int)(5.9604644775390625E-8 * z);
						iq[jz] = (int)(z - 1.6777216E7 * fw);
						jz += 1;
						q0 += 24;
						iq[jz] = (int)fw;
					}
					else
						iq[jz] = (int)z;
				}

				/* convert integer "bit" chunk to floating-point value */
				fw = Math.ScaleB(1.0, q0);
				for (i = jz; i >= 0; i--)
				{
					q[i] = fw * iq[i];
					fw *= 5.9604644775390625E-8;
				}

				/* compute PIo2[0,...,jp]*q[jz,...,0] */
				for (i = jz; i >= 0; i--)
				{
					for (fw = 0.0, k = 0; k <= jp && k <= jz - i; k++)
						fw += Constants.RemPio.PIO2[k] * q[i + k];
					fq[jz - i] = fw;
				}

				switch (prec)
				{
					case 0:
						fw = 0.0;
						for (i = jz; i >= 0; i--)
							fw += fq[i];
						y[0] = ih == 0 ? fw : -fw;
						break;
					case 1:
					case 2:
						fw = 0.0;
						for (i = jz; i >= 0; i--)
							fw += fq[i];
						fw = (double)fw;
						y[0] = ih == 0 ? fw : -fw;
						fw = fq[0] - fw;
						for (i = 1; i <= jz; i++)
							fw += fq[i];
						y[1] = ih == 0 ? fw : -fw;
						break;
					case 3: // Painful
						for (i = jz; i > 0; i--)
						{
							fw = fq[i - 1] + fq[i];
							fq[i] += fq[i - 1] - fw;
							fq[i - 1] = fw;
						}
						for (i = jz; i > 1; i--)
						{
							fw = fq[i - 1] + fq[i];
							fq[i] += fq[i - 1] - fw;
							fq[i - 1] = fw;
						}
						for (fw = 0.0, i = jz; i >= 2; i--)
							fw += fq[i];
						if (ih == 0)
						{
							y[0] = fq[0]; y[1] = fq[1]; y[2] = fw;
						}
						else
						{
							y[0] = -fq[0]; y[1] = -fq[1]; y[2] = -fw;
						}
						break;
				}

				return n & 7;
			}
		}

		/// <summary>
		/// Returns the cosine of the specified angle.
		/// </summary>
		/// <param name="x">An angle, measured in radians.</param>
		/// <returns>The cosine of <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NegativeInfinity"/>, or <see cref="Quad.PositiveInfinity"/>, this method returns <see cref="Quad.NaN"/>.</returns>
		public static Quad Cos(Quad x)
		{
			Quad x0 = x;
			ushort exponent = x.BiasedExponent;
			uint n;
			Span<Quad> y = stackalloc Quad[2];
			Quad hi, lo;

			exponent &= 0x7FFF;
			if (exponent == 0x7FFF)
			{
				return Quad.NaN;
			}
			x = x0;

			if (x < M_PI_4)
			{
				if (exponent < 0x3FF - 113)
				{
					return Quad.One + x;
				}
				return __cos(x, Quad.One);
			}

			n = (uint)__rem_pio2l(x, y);

			hi = y[0];
			lo = y[1];

			return (n & 3) switch
			{
				0 => __cos(hi, lo),
				1 => -__sin(hi, lo, 1),
				2 => -__cos(hi, lo),
				_ => __sin(hi, lo, 1),
			};
		}
		internal static Quad __cos(in Quad x, in Quad y)
		{
			/* origin: FreeBSD /usr/src/lib/msun/ld128/k_cosl.c */
			/*
			 * ====================================================
			 * Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
			 * Copyright (c) 2008 Steven G. Kargl, David Schultz, Bruce D. Evans.
			 *
			 * Developed at SunSoft, y Sun Microsystems, Inc. business.
			 * Permission to use, copy, modify, and distribute this
			 * software is freely granted, provided that this notice
			 * is preserved.
			 * ====================================================
			 */

			Quad hz, z, r, w;

			z = x * x;
			r = (z * (C1 + z * (C2 + z * (C3 + z * (C4 + z * (C5 + z * (C6 + z * (C7 + z * (C8 + z * (C9 + z * (C10 + z * C11)))))))))));
			hz = Quad.HalfOne * z;
			w = Quad.One - hz;

			return w + (((Quad.One - w) - hz) + (z * r - x * y));
		}
		/// <summary>
		/// Returns the hyperbolic cosine of the specified angle.
		/// </summary>
		/// <param name="x">An angle, measured in radians.</param>
		/// <returns>The hyperbolic cosine of <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NegativeInfinity"/> or <see cref="Quad.PositiveInfinity"/>, <see cref="Quad.PositiveInfinity"/> is returned. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NaN"/> is returned.</returns>
		public static Quad Cosh(Quad x)
		{
			var exponent = x.BiasedExponent;
			Quad t;

			x = Quad.Abs(x);

			// |x| < log(2)
			if (exponent < 0x3FFF - 1 || x < new Quad(0x3FFE_62E4_2FEF_A39E, 0xF357_93C7_6730_07E6))
			{
				if (exponent < 0x3FFF - 32)
				{
					return Quad.One;
				}
				t = Quad.ExpM1(x);
				return Quad.One + t * t / (Quad.Two * (Quad.One + t));
			}

			// |x| < log(MaxValue)
			if (exponent < 0x3FFF + 13 || x < new Quad(0x400C_62E4_2FEF_A39E, 0xF357_93C7_6730_07E6))
			{
				t = Exp(x);
				return Quad.HalfOne * (t + ReciprocalEstimate(t));
			}

			// |x| > log(MaxValue) or nan
			t = Exp(Quad.HalfOne * x);
			return Quad.HalfOne * t * t;
		}
		/// <summary>
		/// Returns <seealso cref="Quad.E"/> raised to the specified power.
		/// </summary>
		/// <param name="x">A number specifying a power.</param>
		/// <returns>The number <seealso cref="Quad.E"/> raised to the power <paramref name="x"/>. If <paramref name="x"/> equals <see cref="Quad.NaN"/> or <see cref="Quad.PositiveInfinity"/>, that value is returned. If <paramref name="x"/> equals <see cref="Quad.NegativeInfinity"/>, 0 is returned.</returns>
		public static Quad Exp(Quad x)
		{
			Quad t;

			ushort ix;

			ix = x.BiasedExponent;
			if (ix >= Quad.ExponentBias + 13)
			{
				if (ix == Quad.ExponentBias + Quad.ExponentBias + 1)
				{
					if (Quad.IsNegativeInfinity(x))
					{
						return Quad.Zero;
					}
					return x + x;
				}
				if (x > Constants.Exp.O_THRESHOLD)
				{
					return Quad.PositiveInfinity;
				}
				if (x < Constants.Exp.U_THRESHOLD)
				{
					return Quad.Zero;
				}
			}
			else if (ix < Quad.ExponentBias - 114)
			{
				return Quad.One + x;
			}

			Exp(x, out Quad hi, out Quad lo, out int k);

			t = hi + lo;
			return ScaleB(t, k);
		}
		private static void Exp(Quad x, out Quad hip, out Quad lop, out int kp)
		{
			Quad q, r, r1, t;
			double dr, fn, r2;
			int n, n2;

			/* Reduce x to (k*ln2 + endpoint[n2] + r1 + r2). */
			fn = (double)x * Constants.Exp.INV_L;
			n = (int)fn;
			n2 = (int)((uint)n % Constants.Exp.Intervals);
			/* Depend on the sign bit being propagated: */
			kp = n >> Constants.Exp.Log2Intervals;
			r1 = x - fn * Constants.Exp.L1;
			r2 = fn * -Constants.Exp.L2;
			r = r1 + r2;

			/* Evaluate expl(endpoint[n2] + r1 + r2) = tbl[n2] * expl(r1 + r2). */

			var tbl = Constants.Exp.Table[n2];
			dr = (double)r;
			q = r2 + r * r * (Constants.Exp.A2 + r * (Constants.Exp.A3 + r * (Constants.Exp.A4 + r * (Constants.Exp.A5 + r * (Constants.Exp.A6 +
				dr * (Constants.Exp.A7 + dr * (Constants.Exp.A8 + dr * (Constants.Exp.A9 + dr * Constants.Exp.A10))))))));
			t = tbl.lo + tbl.hi;
			hip = tbl.hi;
			lop = tbl.lo + t * (q + r1);
		}
		/// <summary>
		/// Returns the largest integral value less than or equal to the specified quadruple-precision floating-point number.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point number</param>
		/// <returns>The largest integral value less than or equal to <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NegativeInfinity"/>, or <see cref="Quad.PositiveInfinity"/>, that value is returned.</returns>
		public static Quad Floor(Quad x)
		{
			var exponent = x.BiasedExponent;
			bool sign = Quad.IsNegative(x);
			Quad y;

			if (exponent >= 0x3FFF + Quad.MantissaDigits - 1 || x == Quad.Zero)
			{
				return x;
			}
			// y = int(x) - x, where int(x) is an integer neighbor of x
			Quad toint = Epsilon;
			if (sign)
			{
				y = x - toint + toint - x;
			}
			else
			{
				y = x + toint - toint - x;
			}
			// special case because of non-nearest rounding modes
			if (exponent <= 0x3FFF - 1)
			{
				return sign ? Quad.NegativeOne : Quad.Zero;
			}
			if (y > Quad.Zero)
			{
				return x + y - Quad.One;
			}
			return x + y;
		}
		/// <summary>
		/// Returns (x * y) + z, rounded as one ternary operation.
		/// </summary>
		/// <param name="x">The number to be multiplied with <paramref name="y"/>.</param>
		/// <param name="y">The number to be multiplied with <paramref name="x"/>.</param>
		/// <param name="z">The number to be added to the result of <paramref name="x"/> multiplied by <paramref name="y"/>.</param>
		/// <returns>(x * y) + z, rounded as one ternary operation.</returns>
		public static Quad FusedMultiplyAdd(Quad x, Quad y, Quad z)
		{
			UInt128 result = BitHelper.MulAddQuadBits(Quad.QuadToUInt128Bits(x), Quad.QuadToUInt128Bits(y), Quad.QuadToUInt128Bits(z));

			return Quad.UInt128BitsToQuad(result);
		}
		/// <summary>
		/// Returns the remainder resulting from the division of a specified number by another specified number.
		/// </summary>
		/// <param name="x">A dividend.</param>
		/// <param name="y">A divisor.</param>
		/// <returns>A number equal to <paramref name="x"/> - (<paramref name="y"/> Q), where Q is the quotient of <paramref name="x"/> / <paramref name="y"/> rounded to the nearest integer</returns>
		public static Quad IEEERemainder(Quad x, Quad y)
		{
			UInt128 uiA = Quad.QuadToUInt128Bits(x);
			bool signA = Quad.IsNegative(x);
			short expA = (short)Quad.ExtractBiasedExponentFromBits(uiA);
			UInt128 sigA = Quad.ExtractTrailingSignificandFromBits(uiA);

			UInt128 uiB = Quad.QuadToUInt128Bits(y);
			short expB = (short)Quad.ExtractBiasedExponentFromBits(uiB);
			UInt128 sigB = Quad.ExtractTrailingSignificandFromBits(uiB);

			if (expA == 0x7FFF)
			{
				if ((sigA != UInt128.Zero) || ((expB == 0x7FFF) && (sigB != UInt128.Zero)))
				{
					return Quad.CreateQuadNaN(Quad.IsNegative(y), sigB);
				}
				return Quad.NaN;
			}
			if (expB == 0x7FFF)
			{
				if (sigB != UInt128.Zero)
				{
					return Quad.CreateQuadNaN(Quad.IsNegative(y), sigB);
				}
				return x;
			}

			if (expB == 0)
			{
				if (sigB == UInt128.Zero)
				{
					return Quad.NaN;
				}
				(var exp, sigA) = BitHelper.NormalizeSubnormalF128Sig(sigA);
				expA = (short)exp;
			}

			sigA |= Quad.SignificandSignMask;
			sigB |= Quad.SignificandSignMask;
			UInt128 rem = sigA, altRem;
			int expDiff = expA - expB;
			uint q, recip32;
			if (expDiff < 1)
			{
				if (expDiff < -1)
				{
					return x;
				}
				if (expDiff != 0)
				{
					--expB;
					sigB += sigB;
					q = 0;
				}
				else
				{
					q = sigB <= rem ? 1U : 0U;
					if (q != 0)
					{
						rem -= sigB;
					}
				}
			}
			else
			{
				recip32 = BitHelper.ReciprocalApproximate((uint)(sigB >> 81));
				expDiff -= 30;

				UInt128 term;
				ulong q64;
				while (true)
				{
					q64 = (ulong)(rem >> 83) * recip32;
					if (expDiff < 0)
					{
						break;
					}
					q = (uint)((q64 + 0x80000000) >> 32);
					rem <<= 29;
					term = sigB * q;
					rem -= term;
					if ((rem & Quad.SignMask) != UInt128.Zero)
					{
						rem += sigB;
					}

					expDiff -= 29;
				}
				// ('expDiff' cannot be less than -29 here.)
				Debug.Assert(expDiff >= -29);

				q = (uint)(q64 >> 32) >> (~expDiff & 31);
				rem <<= expDiff + 30;
				term = sigB * q;
				rem -= term;
				if ((rem & Quad.SignMask) != UInt128.Zero)
				{
					altRem = rem + sigB;
					goto selectRem;
				}
			}

			do
			{
				altRem = rem;
				++q;
				rem -= sigB;
			} while ((rem & Quad.SignMask) == UInt128.Zero);
		selectRem:
			UInt128 meanRem = rem + altRem;
			if (((meanRem & Quad.SignMask) != UInt128.Zero)
				|| ((meanRem == UInt128.Zero) && ((q & 1) != 0)))
			{
				rem = altRem;
			}
			bool signRem = signA;
			if ((rem & Quad.SignMask) != UInt128.Zero)
			{
				signRem = !signRem;
				rem = -rem;
			}
			UInt128 resultBits = BitHelper.NormalizeRoundPackQuad(signRem, expB - 1, rem);
			return Quad.UInt128BitsToQuad(resultBits);
		}
		/// <summary>
		/// Returns the base 2 integer logarithm of a specified number.
		/// </summary>
		/// <param name="x">The number whose logarithm is to be found.</param>
		/// <returns>The base 2 integer log of <paramref name="x"/>; that is, (int)log2(<paramref name="x"/>).</returns>
		public static int ILogB(Quad x)
		{
			const int NaN = -1 - 0x7FFF_FFFF;
			int exponent = x.BiasedExponent;


			if (exponent == 0)
			{
				if (x == Quad.Zero)
				{
					return NaN;
				}
				// subnormal x
				Debug.Assert(Quad.IsSubnormal(x));
				return Quad.MinExponent - ((int)UInt128.TrailingZeroCount(x.TrailingSignificand) - Quad.BiasedExponentLength);
			}
			if (exponent == Quad.MaxBiasedExponent)
			{
				return Quad.IsNaN(x) ? NaN : int.MaxValue;
			}

			return exponent - Quad.ExponentBias;
		}
		/// <summary>
		/// Returns the natural (base <c>e</c>) logarithm of a specified number.
		/// </summary>
		/// <param name="x">The number whose logarithm is to be found.</param>
		/// <returns>If positive, 	The natural logarithm of <paramref name="x"/>; that is, ln <paramref name="x"/>, or log e <paramref name="x"/></returns>
		public static Quad Log(Quad x)
		{
			/* origin: FreeBSD /usr/src/lib/msun/ld128/s_logl.c */

			Log(x, out Constants.Log.LD r);

			if (r.LoSet == 0)
			{
				return r.Hi;
			}
			return r.Hi + r.Lo;
		}
		private static void Log(Quad x, out Constants.Log.LD rp)
		{
			/* origin: FreeBSD /usr/src/lib/msun/ld128/s_logl.c */

			rp = default;
			Quad d, valHi, valLo;
			double dd;
			double dk;
			ulong lx;
			int i, k;
			ushort hx = x.BiasedExponent;
			k = -16383;

			if (Quad.IsNegative(x) || Quad.IsNaNOrZero(x))
			{
				if ((hx | x.TrailingSignificand) == UInt128.Zero)
				{
					rp.Hi = Quad.NegativeInfinity;
					return;
				}
				// log(neg or NaN) = qNaN
				rp.Hi = Quad.NaN;
				return;
			}
			if (Quad.IsInfinity(x))
			{
				rp.Hi = x;
				return;
			}
			if (Quad.IsSubnormal(x))
			{ // Scale up x
				x *= new Quad(0x4070_0000_0000_0000, 0x0000_0000_0000_0000);
				k = -16383 - 113;
				lx = (ulong)(x.TrailingSignificand >> 64);
				hx = x.BiasedExponent;
			}
			else
			{
				lx = (ulong)(x.TrailingSignificand >> 64);
			}

			k += hx | (Quad.IsNegative(x) ? 1 << 15 : 0);
			dk = k;

			x = new Quad(false, 0x3fff, x.TrailingSignificand);

			const int L2I = 49 - Constants.Log.Log2Intervals;
			i = (int)((lx + (1 << (L2I - 2))) >> (L2I - 1));

			d = (x - Constants.Log.H(i)) * Constants.Log.G(i) + Constants.Log.E(i);

			/*
			 * Our algorithm depends on exact cancellation of F_lo(i) and
			 * F_hi(i) with dk*ln_2_lo and dk*ln2_hi when k is -1 and i is
			 * at the end of the table.  This and other technical complications
			 * make it difficult to avoid the double scaling in (dk*ln2) *
			 * log(base) for base != e without losing more accuracy and/or
			 * efficiency than is gained.
			 */

			dd = (double)d;
			valLo = d * d * d * (Constants.Log.P3 +
		d * (Constants.Log.P4 + d * (Constants.Log.P5 + d * (Constants.Log.P6 + d * (Constants.Log.P7 + d * (Constants.Log.P8 +
		dd * (Constants.Log.P9 + dd * (Constants.Log.P10 + dd * (Constants.Log.P11 + dd * (Constants.Log.P12 + dd * (Constants.Log.P13 +
		dd * Constants.Log.P14))))))))))) + (Constants.Log.FLo(i) + dk * Constants.Log.LN2LO) + d * d * Constants.Log.P2;
			valHi = d;

			Sum3(ref valHi, ref valLo, Constants.Log.FHi(i) + dk * Constants.Log.LN2HI);

			rp.Hi = valHi;
			rp.Lo = valLo;
			rp.LoSet = 1;
		}
		/// <summary>
		/// Returns the logarithm of a specified number in a specified base.
		/// </summary>
		/// <param name="a">The number whose logarithm is to be found.</param>
		/// <param name="newBase">The base.</param>
		/// <returns></returns>
		public static Quad Log(Quad a, Quad newBase)
		{
			if (Quad.IsNaN(a))
			{
				return a; // IEEE 754-2008: NaN payload must be preserved
			}

			if (Quad.IsNaN(newBase))
			{
				return newBase; // IEEE 754-2008: NaN payload must be preserved
			}

			if (newBase == 1)
			{
				return Quad.NaN;
			}

			if ((a != 1) && ((newBase == 0) || Quad.IsPositiveInfinity(newBase)))
			{
				return Quad.NaN;
			}

			return Log(a) / Log(newBase);
		}
		/// <summary>
		/// Returns the base 10 logarithm of a specified number.
		/// </summary>
		/// <param name="x">A number whose logarithm is to be found.</param>
		/// <returns>The base 10 log of <paramref name="x"/>; that is, log 10<paramref name="x"/>.</returns>
		public static Quad Log10(Quad x)
		{
			Quad hi, lo;

			Log(x, out Constants.Log.LD r);
			if (r.LoSet == 0)
			{
				return r.Hi;
			}

			Quad w = r.Hi + r.Lo;
			r.Lo = (r.Hi - w) + r.Lo;
			r.Hi = w;

			hi = (float)r.Hi;
			lo = r.Lo + (r.Hi - hi);
			return (Constants.Log.InvLn10Hi * hi + (Constants.Log.InvLn10LoPHi * lo + Constants.Log.InvLn10Lo * hi));
		}
		/// <summary>
		/// Returns the base 2 logarithm of a specified number.
		/// </summary>
		/// <param name="x">A number whose logarithm is to be found.</param>
		/// <returns>The base 2 log of <paramref name="x"/>; that is, log 2<paramref name="x"/>.</returns>
		public static Quad Log2(Quad x)
		{
			Quad hi, lo;

			Log(x, out Constants.Log.LD r);
			if (r.LoSet == 0)
			{
				return r.Hi;
			}

			Quad w = r.Hi + r.Lo;
			r.Lo = (r.Hi - w) + r.Lo;
			r.Hi = w;

			hi = (float)r.Hi;
			lo = r.Lo + (r.Hi - hi);
			return (Constants.Log.InvLn2Hi * hi + (Constants.Log.InvLn2LoPHi * lo + Constants.Log.InvLn2Lo * hi));
		}
		/// <summary>
		/// Returns the larger of two quadruple-precision floating-point numbers.
		/// </summary>
		/// <param name="val1">The first of two quadruple-precision floating-point numbers to compare.</param>
		/// <param name="val2">The second of two quadruple-precision floating-point numbers to compare.</param>
		/// <returns>Parameter <paramref name="val1"/> or <paramref name="val2"/>, whichever is larger. If <paramref name="val1"/>, or <paramref name="val2"/>, or both <paramref name="val1"/> and <paramref name="val2"/> are equal to <see cref="Quad.NaN"/>, <see cref="Quad.NaN"/> is returned.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quad Max(Quad val1, Quad val2)
		{
			// This matches the IEEE 754:2019 `maximum` function
			//
			// It propagates NaN inputs back to the caller and
			// otherwise returns the greater of the inputs. It
			// treats +0 as greater than -0 as per the specification.

			if (val1 != val2)
			{
				if (!Quad.IsNaN(val1))
				{
					return val2 < val1 ? val1 : val2;
				}

				return val1;
			}

			return Quad.IsNegative(val2) ? val1 : val2;
		}
		/// <summary>
		/// Returns the larger magnitude of two quadruple-precision floating-point numbers.
		/// </summary>
		/// <param name="x">The first of two quadruple-precision floating-point numbers to compare.</param>
		/// <param name="y">The second of two quadruple-precision floating-point numbers to compare.</param>
		/// <returns>Parameter <paramref name="x"/> or <paramref name="y"/>, whichever has the larger magnitude. If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="Quad.NaN"/>, <see cref="Quad.NaN"/> is returned.</returns>
		public static Quad MaxMagnitude(Quad x, Quad y)
		{
			// This matches the IEEE 754:2019 `maximumMagnitude` function
			//
			// It propagates NaN inputs back to the caller and
			// otherwise returns the input with a greater magnitude.
			// It treats +0 as greater than -0 as per the specification.

			Quad ax = Abs(x);
			Quad ay = Abs(y);

			if ((ax > ay) || Quad.IsNaN(ax))
			{
				return x;
			}

			if (ax == ay)
			{
				return Quad.IsNegative(x) ? y : x;
			}

			return y;
		}
		/// <summary>
		/// Returns the smaller of two quadruple-precision floating-point numbers.
		/// </summary>
		/// <param name="val1">The first of two quadruple-precision floating-point numbers to compare.</param>
		/// <param name="val2">The second of two quadruple-precision floating-point numbers to compare.</param>
		/// <returns>Parameter <paramref name="val1"/> or <paramref name="val2"/>, whichever is smaller. If <paramref name="val1"/>, <paramref name="val2"/>, or both <paramref name="val1"/> and <paramref name="val2"/> are equal to <see cref="Quad.NaN"/>, <see cref="Quad.NaN"/> is returned.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quad Min(Quad val1, Quad val2)
		{
			// This matches the IEEE 754:2019 `minimum` function
			//
			// It propagates NaN inputs back to the caller and
			// otherwise returns the lesser of the inputs. It
			// treats +0 as greater than -0 as per the specification.

			if (val1 != val2)
			{
				if (!Quad.IsNaN(val1))
				{
					return val1 < val2 ? val1 : val2;
				}

				return val1;
			}

			return Quad.IsNegative(val1) ? val1 : val2;
		}
		/// <summary>
		/// Returns the smaller magnitude of two quadruple-precision floating-point numbers.
		/// </summary>
		/// <param name="x">The first of two quadruple-precision floating-point numbers to compare.</param>
		/// <param name="y">The second of two quadruple-precision floating-point numbers to compare.</param>
		/// <returns>Parameter <paramref name="x"/> or <paramref name="y"/>, whichever has the smaller magnitude. If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="Quad.NaN"/>, <see cref="Quad.NaN"/> is returned.</returns>
		public static Quad MinMagnitude(Quad x, Quad y)
		{
			// This matches the IEEE 754:2019 `minimumMagnitude` function
			//
			// It propagates NaN inputs back to the caller and
			// otherwise returns the input with a lesser magnitude.
			// It treats +0 as greater than -0 as per the specification.

			Quad ax = Abs(x);
			Quad ay = Abs(y);

			if ((ax < ay) || Quad.IsNaN(ax))
			{
				return x;
			}

			if (ax == ay)
			{
				return Quad.IsNegative(x) ? x : y;
			}

			return y;
		}
		/// <summary>
		/// Returns a specified number raised to the specified power.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point number to be raised to a power.</param>
		/// <param name="y">A quadruple-precision floating-point number that specifies a power.</param>
		/// <returns>The number <paramref name="x"/> raised to the power <paramref name="y"/>.</returns>
		public static Quad Pow(Quad x, Quad y)
		{
			/* origin: FreeBSD /usr/src/lib/msun/ld128/e_powl.c */

			Quad z, ax, z_h, z_l, p_h, p_l;
			Quad yy1, t1, t2, r, s, t, u, v, w;
			Quad s2, s_h, s_l, t_h, t_l;
			int i, j, k, yisint, n;
			uint ix, iy;
			int hx, hy;
			Constants.Pow.QuadShape o, p, q;

			Unsafe.SkipInit(out p);
			p.Value = x;
			hx = (int)p.UpperHi;
			ix = (uint)(hx & 0x7fffffff);

			Unsafe.SkipInit(out q);
			q.Value = y;
			hy = (int)q.UpperHi;
			iy = (uint)(hy & 0x7fffffff);

			/* y==zero: x**0 = 1 */
			if ((iy | q.UpperLo | q.LowerHi | q.LowerLo) == 0)
			{
				return Quad.One;
			}

			/* 1.0**y = 1; -1.0**+-Inf = 1 */
			if (x == Quad.One)
			{
				return Quad.One;
			}
			if (x == Quad.NegativeOne && iy == 0x7fff0000
				&& (q.UpperLo | q.LowerHi | q.LowerLo) == 0)
			{
				return Quad.One;
			}

			/* +-NaN return x+y */
			if ((ix > 0x7fff0000)
				|| ((ix == 0x7fff0000)
				&& ((p.UpperLo | p.LowerHi | p.LowerLo) != 0))
				|| (iy > 0x7fff0000)
				|| ((iy == 0x7fff0000)
				&& ((q.UpperLo | q.LowerHi | q.LowerLo) != 0))
				)
			{
				return Quad.NaN;
			}

			/* determine if y is an odd int when x < 0
			 * yisint = 0       ... y is not an integer
			 * yisint = 1       ... y is an odd int
			 * yisint = 2       ... y is an even int
			 */
			yisint = 0;
			if (hx < 0)
			{
				if (iy >= 0x4070_0000) /* 2^113 */
				{
					yisint = 2; /* even integer y */
				}
				else if (iy >= 0x3fff0000) /* 1.0 */
				{
					if (Quad.IsInteger(y))
					{
						z = Quad.HalfOne * y;
						if (Quad.IsInteger(z))
						{
							yisint = 2;
						}
						else
						{
							yisint = 1;
						}
					}
				}
			}

			/* special value of y */
			if ((q.UpperLo | q.LowerHi | q.LowerLo) == 0)
			{
				if (iy == 0x7fff0000) /* y is +-inf */
				{
					if (((ix - 0x3FFF_0000) | p.UpperLo | p.LowerHi | p.LowerLo) == 0)
					{
						return Quad.NaN; /* +-1**inf is NaN */
					}
					else if (ix >= 0x3fff0000) /* (|x|>1)**+-inf = inf,0 */
					{
						return (hy >= 0) ? y : Quad.Zero;
					}
					else /* (|x|<1)**-,+inf = inf,0 */
					{
						return (hy < 0) ? -y : Quad.Zero;
					}
				}
				if (iy == 0x3fff0000) /* y is  +-1 */
				{
					if (hy < 0)
					{
						return ReciprocalEstimate(x);
					}
					else
					{
						return x;
					}
				}

				if (hy == 0x40000000)  /* y is  2 */
				{
					return x * x;
				}

				if (hy == 0x3ffe0000) /* y is  0.5 */
				{
					if (hx >= 0) /* x >= +0 */
					{
						Sqrt(x);
					}
				}
			}

			ax = Abs(x);
			/* Special value of x */
			if ((q.UpperLo | q.LowerHi | q.LowerLo) == 0)
			{
				if (ix == 0x7fff0000 || ix == 0 || ix == 0x3fff0000)
				{
					z = ax; /*x is +-0,+-inf,+-1 */
					if (hy < 0)
					{
						z = ReciprocalEstimate(z);
					}
					if (hx < 0)
					{
						if (((ix - 0x3fff_0000) | (uint)yisint) == 0)
						{
							z = Quad.NaN;
						}
						else if (yisint == 1)
						{
							z = -z;
						}
					}
					return z;
				}
			}

			/* (x<0)**(non-int) is NaN */
			if (((((uint)hx >> 31) - 1) | (uint)yisint) == 0)
			{
				return Quad.NaN;
			}

			/* |y| is Huge.
				2^-16495 = 1/2 of smallest representable value.
				If (1 - 1/131072)^y underflows, y > 1.4986e9 */
			if (iy > 0x401d654b)
			{
				Quad huge = Constants.Pow.Huge;
				Quad tiny = Constants.Pow.Tiny;

				if (iy > 0x407d654b)
				{
					if (ix <= 0x3ffeffff)
					{
						return (hy < 0) ? huge * huge : tiny * tiny;
					}
					if (ix >= 0x3fff0000)
					{
						return (hy > 0) ? huge * huge : tiny * tiny;
					}
				}
				/* over/underflow if x is not close to one */
				if (ix < 0x3ffeffff)
					return (hy < 0) ? huge * huge : tiny * tiny;
				if (ix > 0x3fff0000)
					return (hy > 0) ? huge * huge : tiny * tiny;
			}

			n = 0;
			Unsafe.SkipInit(out o);
			/* take care subnormal number */
			if (ix < 0x00010000)
			{
				ax *= Constants.Pow.Two113;
				n -= 113;
				o.Value = ax;
				ix = o.UpperHi;
			}

			n += (int)((ix) >> 16) - 0x3FFF;
			j = (int)(ix & 0x0000ffff);
			/* determine interval */
			ix = (uint)(j | 0x3fff0000); /* normalize ix */
			if (j <= 0x3988)
			{
				k = 0;
			}
			else if (j < 0xbb67)
			{
				k = 1;
			}
			else
			{
				k = 0;
				n += 1;
				ix -= 0x00010000;
			}

			o.Value = ax;
			o.UpperHi = ix;
			ax = o.Value;


			/* compute s = s_h+s_l = (x-1)/(x+1) or (x-1.5)/(x+1.5) */
			u = ax - Constants.Pow.bp[k]; /* bp[0]=1.0, bp[1]=1.5 */
			v = Quad.One / (ax + Constants.Pow.bp[k]);
			s = u * v;
			s_h = s;

			o.Value = s_h;
			o.LowerLo = 0;
			o.LowerHi &= 0xf8000000;
			s_h = o.Value;
			/* t_h=ax+bp[k] High */
			t_h = ax + Constants.Pow.bp[k];
			o.Value = t_h;
			o.LowerLo = 0;
			o.LowerHi &= 0xf8000000;
			t_h = o.Value;
			t_l = ax - (t_h - Constants.Pow.bp[k]);
			s_l = v * ((u - s_h * t_h) - s_h * t_l);
			/* compute log(ax) */
			s2 = s * s;
			u = Constants.Pow.LN[0] + s2 * (Constants.Pow.LN[1] + s2 * (Constants.Pow.LN[2] + s2 * (Constants.Pow.LN[3] + s2 * Constants.Pow.LN[4])));
			v = Constants.Pow.LD[0] + s2 * (Constants.Pow.LD[1] + s2 * (Constants.Pow.LD[2] + s2 * (Constants.Pow.LD[3] + s2 * (Constants.Pow.LD[4] + s2))));
			r = s2 * s2 * u / v;
			r += s_l * (s_h + s);
			s2 = s_h * s_h;
			t_h = new Quad(0x4000_8000_0000_0000, 0x0000_0000_0000_0000) + s2 + r; // t_h = 3.0 + s2 + r
			o.Value = t_h;
			o.LowerLo = 0;
			o.LowerHi &= 0xf8000000;
			t_h = o.Value;
			t_l = r - ((t_h - new Quad(0x4000_8000_0000_0000, 0x0000_0000_0000_0000)) - s2); // t_l = r - ((t_h - 3.0) - s2)
			/* u+v = s*(1+...) */
			u = s_h * t_h;
			v = s_l * t_h + t_l * s;
			/* 2/(3log2)*(s+...) */
			p_h = u + v;
			o.Value = p_h;
			o.LowerLo = 0;
			o.LowerHi &= 0xf8000000;
			p_h = o.Value;
			p_l = v - (p_h - u);
			z_h = Constants.Pow.CP_H * p_h;       /* cp_h+cp_l = 2/(3*log2) */
			z_l = Constants.Pow.CP_L * p_h + p_l * Constants.Pow.CP + Constants.Pow.dp_l[k];
			/* log2(ax) = (s+..)*2/(3*log2) = n + dp_h + z_h + z_l */
			t = n;
			t1 = (((z_h + z_l) + Constants.Pow.dp_h[k]) + t);
			o.Value = t1;
			o.LowerLo = 0;
			o.LowerHi &= 0xf8000000;
			t1 = o.Value;
			t2 = z_l - (((t1 - t) - Constants.Pow.dp_h[k]) - z_h);

			/* s (sign of result -ve**odd) = -1 else = 1 */
			s = Quad.One;
			if (((((uint)hx >> 31) - 1) | (uint)(yisint - 1)) == 0)
			{
				s = Quad.NegativeOne;           /* (-ve)**(odd int) */
			}

			/* split up y into yy1+y2 and compute (yy1+y2)*(t1+t2) */
			yy1 = y;
			o.Value = yy1;
			o.LowerLo = 0;
			o.LowerHi &= 0xf8000000;
			yy1 = o.Value;
			p_l = (y - yy1) * t1 + y * t2;
			p_h = yy1 * t1;
			z = p_l + p_h;
			o.Value = z;
			j = (int)o.UpperHi;
			if (j >= 0x400d0000) /* z >= 16384 */
			{
				Quad huge = Constants.Pow.Huge;
				/* if z > 16384 */
				if (((uint)(j - 0x400d0000) | o.UpperLo | o.LowerHi |
					o.LowerLo) != 0)
				{
					return s * huge * huge;
				}
				else
				{
					if (p_l + Constants.Pow.OVT > z - p_h)
					{
						return s * huge * huge;
					}
				}
			}
			else if ((j & 0x7fffffff) >= 0x400d01b9) /* z <= -16495 */
			{
				Quad tiny = Constants.Pow.Tiny;
				/* z < -16495 */
				if (((uint)(j - 0xc00d01bc) | o.UpperLo | o.LowerHi |
					o.LowerLo) != 0)
				{
					return s * tiny * tiny; /* underflow */
				}
				else
				{
					if (p_l <= z - p_h)
					{
						return s * tiny * tiny; /* underflow */
					}
				}
			}
			/* compute 2**(p_h+p_l) */
			i = j & 0x7fffffff;
			k = (i >> 16) - 0x3fff;
			n = 0;
			if (i > 0x3ffe0000)
			{
				n = (int)Floor(z + Quad.HalfOne);
				t = n;
				p_h -= t;
			}

			t = p_l + p_h;
			o.Value = t;
			o.LowerLo = 0;
			o.LowerHi &= 0xf8000000;
			t = o.Value;
			u = t * Constants.Pow.LG2_H;
			v = (p_l - (t - p_h)) * Constants.Pow.LG2 + t * Constants.Pow.LG2_L;
			z = u + v;
			w = v - (z - u);
			/*  exp(z) */
			t = z * z;
			u = Constants.Pow.PN[0] + t * (Constants.Pow.PN[1] + t * (Constants.Pow.PN[2] + t * (Constants.Pow.PN[3] + t * Constants.Pow.PN[4])));
			v = Constants.Pow.PD[0] + t * (Constants.Pow.PD[1] + t * (Constants.Pow.PD[2] + t * (Constants.Pow.PD[3] + t)));
			t1 = z - t * u / v;
			r = (z * t1) / (t1 - Quad.Two) - (w + z * w);
			z = Quad.One - (r - z);
			o.Value = z;
			j = (int)o.UpperHi;
			j += (n << 16);
			if ((j >> 16) <= 0)
			{
				z = ScaleB(z, n);
			}
			else
			{
				o.UpperHi = (uint)j;
				z = o.Value;
			}
			return s * z;
		}
		/// <summary>
		/// Returns an estimate of the reciprocal of a specified number.
		/// </summary>
		/// <param name="x">The number whose reciprocal is to be estimated.</param>
		/// <returns>An estimate of the reciprocal of <paramref name="x"/>.</returns>
		public static Quad ReciprocalEstimate(Quad x)
		{
			/*
			 * source:
			 * Gaurav Agrawal, Ankit Khandelwal. 
			 * A Newton Raphson Divider Based on 
			 * Improved Reciprocal Approximation Algorithm
			 * http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.134.725&rep=rep1&type=pd
			 */

			if (x == Quad.Zero)
			{
				return Quad.PositiveInfinity;
			}
			if (Quad.IsInfinity(x))
			{
				return Quad.IsNegative(x) ? Quad.NegativeZero : Quad.Zero;
			}
			if (Quad.IsNaN(x))
			{
				return x;
			}

			// Uses Newton Raphton Series to find 1/y
			Quad x0;
			var bits = Quad.ExtractFromBits(Quad.QuadToUInt128Bits(x));

			// we save the original sign and exponent for later
			bool sign = bits.sign;
			ushort exp = bits.exponent;

			// Expresses D as M × 2e where 1 ≤ M < 2
			// we also get the absolute value while we are at it.
			Quad normalizedValue = new Quad(true, Quad.ExponentBias, bits.matissa);

			x0 = Quad.One;
			Quad two = Quad.Two;

			// 15 iterations should be enough.
			for (int i = 0; i < 15; i++)
			{
				// X1 = X(2 - DX)
				// x1 = f * (two - (normalizedValue * f))
				Quad x1 = x0 * FusedMultiplyAdd(normalizedValue, x0, two);
				// Since we need: two - (normalizedValue * f)
				// to make use of FusedMultiplyAdd, we can rewrite it to (-normalizedValue * f) + two
				// which requires normalizedValue to be negative...

				if (Quad.Abs(x1 - x) < Quad.Epsilon)
				{
					x0 = x1;
					break;
				}

				x0 = x1;
			}

			bits = Quad.ExtractFromBits(Quad.QuadToUInt128Bits(x0));

			bits.exponent -= (ushort)(exp - Quad.ExponentBias);

			var output = new Quad(sign, bits.exponent, bits.matissa);
			return output;
		}
		/// <summary>
		/// Returns an estimate of the reciprocal square root of a specified number.
		/// </summary>
		/// <param name="x">The number whose reciprocal square root is to be estimated.</param>
		/// <returns>An estimate of the reciprocal square root <paramref name="x"/>.</returns>
		public static Quad ReciprocalSqrtEstimate(Quad x) => ReciprocalEstimate(Sqrt(x));
		/// <summary>
		/// Rounds a quadruple-precision floating-point value to the nearest integral value, and rounds midpoint values to the nearest even number.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point number to be rounded.</param>
		/// <returns>The integer nearest <paramref name="x"/>. If the fractional component of <paramref name="x"/> is halfway between two integers, one of which is even and the other odd, then the even number is returned.</returns>
		public static Quad Round(Quad x)
		{
			// This is based on the 'Berkeley SoftFloat Release 3e' algorithm
			// source: berkeley-softfloat-3/source/f128_roundToInt.c

			UInt128 uiZ;
			UInt128 bits = Quad.QuadToUInt128Bits(x);
			ulong uiZ64 = x._upper, uiZ0 = x._lower, roundBitsMask;
			ulong lastBitMask64, lastBitMask0;
			ushort biasedExponent = Quad.ExtractBiasedExponentFromBits(bits);

			if (biasedExponent >= 0x402F)
			{
				if (biasedExponent >= 0x406F)
				{
					if (biasedExponent == 0x7FFF && (Quad.ExtractTrailingSignificandFromBits(bits) != UInt128.Zero))
					{
						return Quad.NaN;
					}
					return x;
				}

				lastBitMask0 = (ulong)(2 << (0x406E - biasedExponent));
				roundBitsMask = lastBitMask0 - 1;
				uiZ = bits;

				if (biasedExponent == 0x402F)
				{
					if (uiZ0 >= 0x8000_0000_0000_0000)
					{
						uiZ64++;
						if (uiZ0 == 0x8000_0000_0000_0000)
						{
							uiZ64 &= ~1UL;
						}
					}
				}
				else
				{
					uiZ = new UInt128(uiZ64, uiZ0) + new UInt128(0, lastBitMask0 >> 1);
					if (((ulong)uiZ & roundBitsMask) == 0)
					{
						uiZ &= new UInt128(0xFFFF_FFFF_FFFF_FFFF, ~lastBitMask0);
					}
				}

				uiZ &= new UInt128(0xFFFF_FFFF_FFFF_FFFF, ~roundBitsMask);

				lastBitMask64 = (lastBitMask0 == 0) ? 0UL : 1UL;
			}
			else
			{
				if (biasedExponent < 0x3FFF)
				{
					if ((bits & new UInt128(0x7FFF_FFFF_FFFF_FFFF, 0xFFFF_FFFF_FFFF_FFFF)) == UInt128.Zero)
					{
						return x;
					}
					uiZ = bits & new UInt128(BitHelper.PackToQuadUI64(true, 0, 0), 0);

					return Quad.UInt128BitsToQuad(uiZ);
				}

				uiZ = bits & new UInt128(0xFFFF_FFFF_FFFF_FFFF, 0x0);
				lastBitMask64 = (ulong)1 << (0x402F - biasedExponent);
				roundBitsMask = lastBitMask64 - 1;
				uiZ += new UInt128(lastBitMask64 >> 1, 0);
				if ((((ulong)(uiZ >> 64) & roundBitsMask) | (ulong)bits) == 0)
				{
					uiZ &= new UInt128(~lastBitMask64, 0xFFFF_FFFF_FFFF_FFFF);
				}
				uiZ &= new UInt128(~roundBitsMask, 0xFFFF_FFFF_FFFF_FFFF);
				lastBitMask0 = 0;
			}

			return Quad.UInt128BitsToQuad(uiZ);
		}
		/// <summary>
		/// Rounds a quadruple-precision floating-point value to a specified number of fractional digits, and rounds midpoint values to the nearest even number.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point number to be rounded.</param>
		/// <param name="digits">The number of fractional digits in the return value.</param>
		/// <returns>The number nearest to <paramref name="x"/> that contains a number of fractional digits equal to <paramref name="digits"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="digits"/> is less than 0 or greater than 34.</exception>
		public static Quad Round(Quad x, int digits)
		{
			return Round(x, digits, MidpointRounding.ToEven);
		}
		/// <summary>
		/// Rounds a quadruple-precision floating-point value to an integer using the specified rounding convention.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point number to be rounded.</param>
		/// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
		/// <returns>The integer that <paramref name="x"/> is rounded to using the <paramref name="mode"/> rounding convention. This method returns a <see cref="Quad"/> instead of an integral type.</returns>
		/// <exception cref="ArgumentException"><paramref name="mode"/> is not a valid value of <seealso cref="MidpointRounding"/>.</exception>
		public static Quad Round(Quad x, MidpointRounding mode)
		{
			return Round(x, 0, mode);
		}
		/// <summary>
		/// Rounds a quadruple-precision floating-point value to a specified number of fractional digits using the specified rounding convention.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point number to be rounded.</param>
		/// <param name="digits">The number of fractional digits in the return value.</param>
		/// <param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
		/// <returns>The number that <paramref name="x"/> is rounded to that has <paramref name="digits"/> fractional digits. If <paramref name="x"/> has fewer fractional digits than <paramref name="digits"/>, <paramref name="x"/> is returned unchanged.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="digits"/> is less than 0 or greater than 34.</exception>
		/// <exception cref="ArgumentException"><paramref name="mode"/> is not a valid value of <seealso cref="MidpointRounding"/>.</exception>
		public static Quad Round(Quad x, int digits, MidpointRounding mode)
		{
			if ((uint)digits > MaxRoundingDigits)
			{
				Thrower.OutOfRange(nameof(digits));
			}

			if (Abs(x) < RoundLimit)
			{
				Quad power10 = RoundPower10[digits];

				x *= power10;

				x = mode switch
				{
					MidpointRounding.ToEven => Round(x),
					MidpointRounding.AwayFromZero => Truncate(x + CopySign(BitDecrement(Quad.HalfOne), x)),
					MidpointRounding.ToZero => Truncate(x),
					MidpointRounding.ToNegativeInfinity => Floor(x),
					MidpointRounding.ToPositiveInfinity => Ceiling(x),
					_ => throw new ArgumentException("Invalid enum value.", nameof(mode)),
				};

				x /= power10;
			}

			return x;
		}
		/// <summary>
		/// Returns x * 2^n computed efficiently.
		/// </summary>
		/// <param name="x">A quadruple-precision floating-point number that specifies the base value.</param>
		/// <param name="n">A number that specifies the power.</param>
		/// <returns>x * 2^n computed efficiently.</returns>
		public static Quad ScaleB(Quad x, int n)
		{
			if (n > Quad.MaxExponent)
			{
				Quad maxExp = new Quad(0x7FFE_0000_0000_0000, 0x0000_0000_0000_0000);

				x *= maxExp;
				n -= Quad.MaxExponent;
				if (n > Quad.MaxExponent)
				{
					x *= maxExp;
					n -= Quad.MaxExponent;

					if (n > Quad.MaxExponent)
					{
						n = Quad.MaxExponent;
					}
				}
			}
			else if (n < Quad.MinExponent)
			{
				Quad minExp = new Quad(0x0001_0000_0000_0000, 0x0000_0000_0000_0000);
				Quad b113 = new Quad(0x4070_0000_0000_0000, 0x0000_0000_0000_0000);

				Quad scaleb = minExp * b113;
				x *= scaleb;
				n += -Quad.MinExponent - Quad.MantissaDigits;

				if (n < Quad.MinExponent)
				{
					x *= scaleb;
					n += -Quad.MinExponent - Quad.MantissaDigits;

					if (n < Quad.MinExponent)
					{
						n = Quad.MinExponent;
					}
				}

				Quad result = x * new Quad((ulong)(0x3FFF + n) << 48, 0x0000_0000_0000_0000);
				if (Quad.IsInfinity(result))
				{
					return Quad.Zero;
				}
				return result;
			}

			return x * new Quad((ulong)(0x3FFF + n) << 48, 0x0000_0000_0000_0000);
		}
		/// <summary>
		/// Returns an integer that indicates the sign of a quadruple-precision floating-point number.
		/// </summary>
		/// <param name="x">A signed number.</param>
		/// <returns>A number that indicates the sign of <paramref name="x"/>.</returns>
		/// <exception cref="ArithmeticException"><paramref name="x"/> is equal to <see cref="Quad.NaN"/>.</exception>
		public static int Sign(Quad x)
		{
			if (x < Quad.Zero)
			{
				return -1;
			}
			else if (x > Quad.Zero)
			{
				return 1;
			}
			else if (x == Quad.Zero)
			{
				return 0;
			}

			Thrower.InvalidNaN(x);
			return default;
		}
		/// <summary>
		/// Returns the sine of the specified angle.
		/// </summary>
		/// <param name="x">An angle, measured in radians.</param>
		/// <returns>The sine of <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NegativeInfinity"/>, or <see cref="Quad.PositiveInfinity"/>, this method returns <see cref="Quad.NaN"/>.</returns>
		public static Quad Sin(Quad x)
		{
			Quad x0 = x;
			Shape u = new Shape(ref x0);
			int n;
			Quad hi, lo;
			Span<Quad> y = stackalloc Quad[2];

			u.i.se &= 0x7fff;
			if (u.i.se == 0x7fff)
				return x - x;
			if (x0 < M_PI_4)
			{
				if (u.i.se < 0x3fff - Quad.MantissaDigits / 2)
				{
					return x;
				}

				return __sin(x, Quad.Zero, 0);
			}
			n = __rem_pio2l(x, y);
			hi = y[0];
			lo = y[1];

			return (n & 3) switch
			{
				0 => __sin(hi, lo, 1),
				1 => __cos(in hi, in lo),
				2 => -__sin(hi, lo, 1),
				_ => -__cos(in hi, in lo),
			};
		}
		internal static Quad __sin(Quad x, Quad y, int iy)
		{
			Quad z, r, v;

			z = x * x;
			v = z * x;
			r = (S2 + z * (S3 + z * (S4 + z * (S5 + z * (S6 + z * (S7 + z * (S8 + z * (S9 + z * (S10 + z * (S11 + z * S12))))))))));

			if (iy == 0)
			{
				return x + v * (S1 + z * r);
			}

			return x - ((z * (Quad.HalfOne * y - v * r) - y) - v * S1);
		}
		/// <summary>
		/// Returns the sine and cosine of the specified angle.
		/// </summary>
		/// <param name="x">An angle, measured in radians.</param>
		/// <returns>The sine and cosine of <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NegativeInfinity"/>, or <see cref="Quad.PositiveInfinity"/>, this method returns <see cref="Quad.NaN"/>.</returns>
		public static (Quad Sin, Quad Cos) SinCos(Quad x)
		{
			uint n;
			Span<Quad> y = stackalloc Quad[2];
			Quad s, c;

			var se = x.BiasedExponent;
			if (se == 0x7FFF)
			{
				return (x, x);
			}
			if (Abs(x) < M_PI_4)
			{
				if (se < 0x3FFF - Quad.MantissaDigits)
				{
					// raise underflow if subnormal
					return (x, Quad.One + x);
				}
				return (__sin(x, Quad.Zero, 0), __cos(in x, Quad.Zero));
			}
			n = (uint)__rem_pio2l(x, y);
			s = __sin(y[0], y[1], 1);
			c = __cos(in y[0], in y[1]);

			switch (n & 3)
			{
				case 0:
					return (s, c);
				case 1:
					return (c, -s);
				case 2:
					return (-s, -c);
				case 3:
				default:
					return (-c, s);
			}
		}
		/// <summary>
		/// Returns the hyperbolic sine of the specified angle.
		/// </summary>
		/// <param name="x">An angle, measured in radians.</param>
		/// <returns>The hyperbolic sine of <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NegativeInfinity"/>, or <see cref="Quad.PositiveInfinity"/>, this method returns a <see cref="Quad"/> equal to <paramref name="x"/>.</returns>
		public static Quad Sinh(Quad x)
		{
			var ex = x.BiasedExponent;
			Quad h, t, absx;

			h = Quad.HalfOne;
			if (Quad.IsNegative(x))
			{
				h = -h;
			}

			absx = Abs(x);

			// |x| < log(LDBL_MAX)
			if (absx < new Quad(0x400C_62E4_2FEF_A39E, 0xF357_93C7_6730_07E6))
			{
				t = Quad.ExpM1(absx);
				if (ex < 0x3FFF)
				{
					if (ex < 0x3FFF - 32)
					{
						return x;
					}
					return h * (Quad.Two * t - t * t / (Quad.One + t));
				}
				return h * (t + t / (t + Quad.One));
			}

			// |x| > log(LDBL_MAX) or nan

			t = Exp(Quad.HalfOne * absx);
			return h * t * t;
		}
		/// <summary>
		/// Returns the square root of a specified number.
		/// </summary>
		/// <param name="x">The number whose square root is to be found.</param>
		/// <returns>The positive square root of <paramref name="x"/>.</returns>
		public static Quad Sqrt(Quad x)
		{
			// This is based on the 'Berkeley SoftFloat Release 3e' algorithm
			// source: berkeley-softfloat-3/source/f128_sqrt.c

			UInt128 bits = Quad.QuadToUInt128Bits(x);
			bool signA = Quad.IsNegative(x);
			int exp = Quad.ExtractBiasedExponentFromBits(bits);
			UInt128 sig = Quad.ExtractTrailingSignificandFromBits(bits);

			// Is x NaN?
			if (exp == 0x7FFF)
			{
				if (sig != UInt128.Zero)
				{
					return Quad.NaN;
				}
				if (!signA)
				{
					return x;
				}
				return Quad.NaN;
			}
			if (signA)
			{
				if (((UInt128)exp | sig) == UInt128.Zero)
				{
					return x;
				}
				return Quad.NaN;
			}

			if (exp == 0)
			{
				if (sig == UInt128.Zero)
				{
					return x;
				}
				(exp, sig) = BitHelper.NormalizeSubnormalF128Sig(sig);
			}

			/*
			 * `sig32Z' is guaranteed to be a lower bound on the square root of
			 * `sig32A', which makes `sig32Z' also a lower bound on the square root of
			 * `sigA'.
			 */

			int expZ = ((exp - 0x3FFF) >> 1) + 0x3FFE;
			exp &= 1;
			sig |= new UInt128(0x0001000000000000, 0x0);
			uint sig32 = (uint)(sig >> 81);
			uint recipSqrt32 = BitHelper.SqrtReciprocalApproximate((uint)exp, sig32);
			uint sig32Z = (uint)(((ulong)sig32 * recipSqrt32) >> 32);
			UInt128 rem;
			if (exp != 0)
			{
				sig32Z >>= 1;
				rem = sig << 12;
			}
			else
			{
				rem = sig << 13;
			}
			Span<uint> qs = [0, 0, sig32Z];
			rem -= new UInt128((ulong)sig32Z * sig32Z, 0x0);

			uint q = (uint)(((uint)(rem >> 66) * (ulong)recipSqrt32) >> 32);
			ulong x64 = (ulong)sig32Z << 32;
			ulong sig64Z = x64 + ((ulong)q << 3);
			UInt128 y = rem << 29;

			UInt128 term;
			do
			{
				term = BitHelper.Mul64ByShifted32To128(x64 + sig64Z, q);
				rem = y - term;
				if (((rem.GetUpperBits()) & 0x8000_0000_0000_0000) == 0)
				{
					break;
				}
				--q;
				sig64Z -= 1 << 3;
			} while (true);
			qs[1] = q;

			q = (uint)(((ulong)(rem >> 66) * recipSqrt32) >> 32);
			y = rem << 29;
			sig64Z <<= 1;

			do
			{
				term = (UInt128)sig64Z << 32;
				term += (ulong)q << 6;
				term *= q;
				rem = y - term;
				if (((ulong)(rem >> 64) & 0x8000_0000_0000_0000) == 0)
				{
					break;
				}
				--q;
			} while (true);
			qs[0] = q;

			q = (uint)((((ulong)(rem >> 66) * recipSqrt32) >> 32) + 2);
			ulong sigZExtra = (ulong)q << 59;
			term = (UInt128)qs[1] << 53;
			UInt128 sigZ = new UInt128((ulong)qs[2] << 18, ((ulong)qs[0] << 24) + (q >> 5)) + term;

			if ((q & 0xF) <= 2)
			{
				q &= ~3U;
				sigZExtra = (ulong)q << 59;
				y = sigZ << 6;
				y |= sigZExtra >> 58;
				term = y - q;
				y = BitHelper.Mul64ByShifted32To128((ulong)term, q);
				term = BitHelper.Mul64ByShifted32To128((ulong)(term >> 64), q);
				term += (y >> 64);
				rem <<= 20;
				term -= rem;
				/*
				 * The concatenation of `term' and `x.v0' is now the negative remainder
				 * (3 words altogether).
				 */
				if ((term.GetUpperBits() & 0x8000_0000_0000_0000) != 0)
				{
					sigZExtra |= 1;
				}
				else
				{
					if ((term | y.GetLowerBits()) != UInt128.Zero)
					{
						if (sigZExtra != 0)
						{
							--sigZExtra;
						}
						else
						{
							sigZ -= UInt128.One;
							sigZExtra = ~0UL;
						}
					}
				}
			}

			return Quad.UInt128BitsToQuad(BitHelper.RoundPackToQuad(false, expZ, sigZ, sigZExtra));
		}
		/// <summary>
		/// Returns the tangent of the specified angle.
		/// </summary>
		/// <param name="x">An angle, measured in radians.</param>
		/// <returns>The tangent of <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, <see cref="Quad.NegativeInfinity"/>, or <see cref="Quad.PositiveInfinity"/>, this method returns <see cref="Quad.NaN"/>.</returns>
		public static Quad Tan(Quad x)
		{
			Quad x0 = x;
			Shape u = new Shape(ref x0);
			Span<Quad> y = stackalloc Quad[2];
			int n;

			u.i.se &= 0x7FFF;
			if (u.i.se == 0x7FFF)
			{
				return x - x;
			}
			if (x0 < M_PI_4)
			{
				if (u.i.se < 0x3FFF - Quad.MantissaDigits / 2)
				{
					return x;
				}
				return __tan(x, Quad.Zero, 0);
			}
			n = __rem_pio2l(x, y);
			return __tan(y[0], y[1], n & 1);
		}
		internal static Quad __tan(Quad x, Quad y, int odd)
		{
			Quad z, r, v, w, s, a, t;
			bool big, sign = false;

			big = Abs(x) >= new Quad(0x3FFE_5943_17AC_C4EF, 0x88B9_7785_729B_280F); // |x| >= 0.67434
			if (big)
			{
				if (x < Quad.Zero)
				{
					sign = true;
					x = -x;
					y = -y;
				}
				x = (Pio4 - x) + (PIO4LO - y);
				y = Quad.Zero;
			}

			z = x * x;
			w = z * z;
			r = RPoly(w);
			v = z * VPoly(w);
			s = z * x;
			r = y + z * (s * (r + v) + y) + T3 * s;
			w = x + r;

			if (big)
			{
				s = 1 - 2 * odd;
				v = s - Quad.Two * (x + (r - w * w / (w + s)));
				return sign ? -v : v;
			}
			if (odd == 0)
			{
				return w;
			}

			/*
			 * if allow error up to 2 ulp, simply return
			 * -1.0 / (x+r) here
			 *
			 * 
			 * compute -1.0 / (x+r) accurately 
			 */

			Quad temp = new Quad(0x401F_0000_0000_0000, 0x0000_0000_0000_0000);
			z = w;
			z = z + temp - temp;
			v = r - (z - x);        /* z+v = r+x */
			t = a = Quad.NegativeOne / w;       /* x = -1.0/w */
			t = t + temp - temp;
			s = Quad.One + t * z;
			return t + a * (s + t * v);

			static Quad RPoly(Quad w)
			{
				return (T5 + w * (T9 + w * (T13 + w * (T17 + w * (T21 +
					w * (T25 + w * (T29 + w * (T33 + w * (T37 + w * (T41 +
					w * (T45 + w * (T49 + w * (T53 + w * T57)))))))))))));
			}
			static Quad VPoly(Quad w)
			{
				return (T7 + w * (T11 + w * (T15 + w * (T19 + w * (T23 +
					w * (T27 + w * (T31 + w * (T35 + w * (T39 + w * (T43 +
					w * (T47 + w * (T51 + w * T55))))))))))));
			}
		}
		/// <summary>
		/// Returns the hyperbolic tangent of the specified angle.
		/// </summary>
		/// <param name="x">An angle, measured in radians.</param>
		/// <returns>The hyperbolic tangent of <paramref name="x"/>. If <paramref name="x"/> is equal to <see cref="Quad.NegativeInfinity"/>, this method returns -1. If value is equal to <see cref="Quad.PositiveInfinity"/>, this method returns 1. If <paramref name="x"/> is equal to <see cref="Quad.NaN"/>, this method returns <see cref="Quad.NaN"/>.</returns>
		public static Quad Tanh(Quad x)
		{
			var ex = x.BiasedExponent;
			var sign = Quad.IsNegative(x);
			Quad t;

			x = Quad.Abs(x);

			if (x > new Quad(0x3FFE_193E_A7AA_D030, 0xA976_A419_8D55_053B))
			{
				// |x| > log(3)/2
				if (ex >= 0x3FFF + 5)
				{
					// |x| >= 32
					t = Quad.One;
				}
				else
				{
					t = Quad.ExpM1(Quad.Two * x);
					t = Quad.One - Quad.Two / (t + Quad.Two);
				}
			}
			else if (x > new Quad(0x3FFD_058A_EFA8_1145, 0x1A72_76BC_2F82_043B))
			{
				// |x| > log(5/3)/2
				t = Quad.ExpM1(Quad.Two * x);
				t = t / (t + Quad.Two);
			}
			else
			{
				// |x| is small
				t = Quad.ExpM1(-Quad.Two * x);
				t = -t / (t + Quad.Two);
			}

			return sign ? -t : t;
		}
		/// <summary>
		/// Calculates the integral part of a specified quadruple-precision floating-point number.
		/// </summary>
		/// <param name="x">A number to truncate.</param>
		/// <returns>The integral part of <paramref name="x"/>; that is, the number that remains after any fractional digits have been discarded, or one of the values listed in the following table.</returns>
		public static Quad Truncate(Quad x)
		{
			var exponent = x.BiasedExponent;
			bool sign = Quad.IsNegative(x);

			Quad y;

			if (exponent >= 0x3FFF + Quad.MantissaDigits - 1)
			{
				return x;
			}
			if (exponent <= 0x3FFF - 1)
			{
				return sign ? Quad.NegativeZero : Quad.Zero;
			}
			// y = int(|x|) - |x|, where int(|x|) is an integer neighbor of |x|
			if (sign)
			{
				x = -x;
			}
			Quad toint = Epsilon;
			y = x + toint - toint - x;
			if (y > 0)
			{
				y--;
			}
			x += y;
			return sign ? -x : x;
		}

		internal static Quad ModF(Quad x, out Quad iptr)
		{
			int e = x.Exponent;
			bool s = Quad.IsNegative(x);
			Quad absX;
			Quad y;

			// no fractional part
			if (e >= Quad.MantissaDigits - 1)
			{
				iptr = x;
				if (Quad.IsNaN(x))
				{
					return x;
				}
				return s ? Quad.NegativeZero : Quad.Zero;
			}

			// no integral part
			if (e < 0)
			{
				iptr = s ? Quad.NegativeZero : Quad.Zero;
				return x;
			}

			// raises spurious inexact
			absX = Quad.Abs(x);
			Quad toint = Epsilon;
			y = absX + toint - toint - absX;
			if (y == Quad.Zero)
			{
				iptr = x;
				return s ? Quad.NegativeZero : Quad.Zero;
			}
			if (y > Quad.Zero)
			{
				y -= Quad.One;
			}
			if (s)
			{
				y = -y;
			}
			iptr = x + y;
			return -y;
		}

		internal static void Sum2(ref Quad a, ref Quad b)
		{
			Quad w;

			w = a + b;
			b = (a - w) + b;
			a = w;
		}
		internal static void Sum3(ref Quad a, ref Quad b, double c)
		{
			Quad tmp;

			tmp = c;
			Sum2(ref tmp, ref a);
			b += a;
			a = tmp;
		}
	}
}
