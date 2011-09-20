
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

namespace FlowLib.Utils.Convert
{
    public class Setting
    {
        public enum Types
        {
            All = 0,
            Hub,
            General,
            Share
        }

        public enum Clients
        {
            AirDC_2_01,
            Apex_0_40,
            BCDC_0_699d,
            CrZDC_Beta3,
            CrZDC_0_699b,
            DCpp,
            DCpp0_403,
            DCDM = DCpp0_403,
            fulDC_6_78,
            IceDC_1_00a,
            LDC_1_00v2a,
            RSX_1_00,
            StrongDC_2_1,
            StrongDC_Lite_131,
            ZionBlue_2_01,
            ZionBlue_2_02,
            ZionBlue_2_03,
            ZionBlue_2_04,
            ZionBlue_2_05,
            zK_0_666,
            zK_0_7,
            zK_0_710
        }
    }
}
