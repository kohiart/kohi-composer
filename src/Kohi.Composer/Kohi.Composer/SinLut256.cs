﻿// Copyright (c) Kohi Art Community, Inc.

namespace Kohi.Composer;

public static class SinLut256
{
    public static long SinLut(long i)
    {
        if (i <= 127)
        {
            if (i <= 63)
            {
                if (i <= 31)
                {
                    if (i <= 15)
                    {
                        if (i <= 7)
                        {
                            if (i <= 3)
                            {
                                if (i <= 1)
                                {
                                    if (i == 0)
                                        return 0;
                                    return 26456769;
                                }

                                if (i == 2)
                                    return 52912534;
                                return 79366292;
                            }

                            if (i <= 5)
                            {
                                if (i == 4)
                                    return 105817038;
                                return 132263769;
                            }

                            if (i == 6)
                                return 158705481;
                            return 185141171;
                        }

                        if (i <= 11)
                        {
                            if (i <= 9)
                            {
                                if (i == 8)
                                    return 211569835;
                                return 237990472;
                            }

                            if (i == 10)
                                return 264402078;
                            return 290803651;
                        }

                        if (i <= 13)
                        {
                            if (i == 12)
                                return 317194190;
                            return 343572692;
                        }

                        if (i == 14)
                            return 369938158;
                        return 396289586;
                    }

                    if (i <= 23)
                    {
                        if (i <= 19)
                        {
                            if (i <= 17)
                            {
                                if (i == 16)
                                    return 422625977;
                                return 448946331;
                            }

                            if (i == 18)
                                return 475249649;
                            return 501534935;
                        }

                        if (i <= 21)
                        {
                            if (i == 20)
                                return 527801189;
                            return 554047416;
                        }

                        if (i == 22)
                            return 580272619;
                        return 606475804;
                    }

                    if (i <= 27)
                    {
                        if (i <= 25)
                        {
                            if (i == 24)
                                return 632655975;
                            return 658812141;
                        }

                        if (i == 26)
                            return 684943307;
                        return 711048483;
                    }

                    if (i <= 29)
                    {
                        if (i == 28)
                            return 737126679;
                        return 763176903;
                    }

                    if (i == 30)
                        return 789198169;
                    return 815189489;
                }

                if (i <= 47)
                {
                    if (i <= 39)
                    {
                        if (i <= 35)
                        {
                            if (i <= 33)
                            {
                                if (i == 32)
                                    return 841149875;
                                return 867078344;
                            }

                            if (i == 34)
                                return 892973912;
                            return 918835595;
                        }

                        if (i <= 37)
                        {
                            if (i == 36)
                                return 944662413;
                            return 970453386;
                        }

                        if (i == 38)
                            return 996207534;
                        return 1021923881;
                    }

                    if (i <= 43)
                    {
                        if (i <= 41)
                        {
                            if (i == 40)
                                return 1047601450;
                            return 1073239268;
                        }

                        if (i == 42)
                            return 1098836362;
                        return 1124391760;
                    }

                    if (i <= 45)
                    {
                        if (i == 44)
                            return 1149904493;
                        return 1175373592;
                    }

                    if (i == 46)
                        return 1200798091;
                    return 1226177026;
                }

                if (i <= 55)
                {
                    if (i <= 51)
                    {
                        if (i <= 49)
                        {
                            if (i == 48)
                                return 1251509433;
                            return 1276794351;
                        }

                        if (i == 50)
                            return 1302030821;
                        return 1327217884;
                    }

                    if (i <= 53)
                    {
                        if (i == 52)
                            return 1352354586;
                        return 1377439973;
                    }

                    if (i == 54)
                        return 1402473092;
                    return 1427452994;
                }

                if (i <= 59)
                {
                    if (i <= 57)
                    {
                        if (i == 56)
                            return 1452378731;
                        return 1477249357;
                    }

                    if (i == 58)
                        return 1502063928;
                    return 1526821503;
                }

                if (i <= 61)
                {
                    if (i == 60)
                        return 1551521142;
                    return 1576161908;
                }

                if (i == 62)
                    return 1600742866;
                return 1625263084;
            }

            if (i <= 95)
            {
                if (i <= 79)
                {
                    if (i <= 71)
                    {
                        if (i <= 67)
                        {
                            if (i <= 65)
                            {
                                if (i == 64)
                                    return 1649721630;
                                return 1674117578;
                            }

                            if (i == 66)
                                return 1698450000;
                            return 1722717974;
                        }

                        if (i <= 69)
                        {
                            if (i == 68)
                                return 1746920580;
                            return 1771056897;
                        }

                        if (i == 70)
                            return 1795126012;
                        return 1819127010;
                    }

                    if (i <= 75)
                    {
                        if (i <= 73)
                        {
                            if (i == 72)
                                return 1843058980;
                            return 1866921015;
                        }

                        if (i == 74)
                            return 1890712210;
                        return 1914431660;
                    }

                    if (i <= 77)
                    {
                        if (i == 76)
                            return 1938078467;
                        return 1961651733;
                    }

                    if (i == 78)
                        return 1985150563;
                    return 2008574067;
                }

                if (i <= 87)
                {
                    if (i <= 83)
                    {
                        if (i <= 81)
                        {
                            if (i == 80)
                                return 2031921354;
                            return 2055191540;
                        }

                        if (i == 82)
                            return 2078383740;
                        return 2101497076;
                    }

                    if (i <= 85)
                    {
                        if (i == 84)
                            return 2124530670;
                        return 2147483647;
                    }

                    if (i == 86)
                        return 2170355138;
                    return 2193144275;
                }

                if (i <= 91)
                {
                    if (i <= 89)
                    {
                        if (i == 88)
                            return 2215850191;
                        return 2238472027;
                    }

                    if (i == 90)
                        return 2261008923;
                    return 2283460024;
                }

                if (i <= 93)
                {
                    if (i == 92)
                        return 2305824479;
                    return 2328101438;
                }

                if (i == 94)
                    return 2350290057;
                return 2372389494;
            }

            if (i <= 111)
            {
                if (i <= 103)
                {
                    if (i <= 99)
                    {
                        if (i <= 97)
                        {
                            if (i == 96)
                                return 2394398909;
                            return 2416317469;
                        }

                        if (i == 98)
                            return 2438144340;
                        return 2459878695;
                    }

                    if (i <= 101)
                    {
                        if (i == 100)
                            return 2481519710;
                        return 2503066562;
                    }

                    if (i == 102)
                        return 2524518435;
                    return 2545874514;
                }

                if (i <= 107)
                {
                    if (i <= 105)
                    {
                        if (i == 104)
                            return 2567133990;
                        return 2588296054;
                    }

                    if (i == 106)
                        return 2609359905;
                    return 2630324743;
                }

                if (i <= 109)
                {
                    if (i == 108)
                        return 2651189772;
                    return 2671954202;
                }

                if (i == 110)
                    return 2692617243;
                return 2713178112;
            }

            if (i <= 119)
            {
                if (i <= 115)
                {
                    if (i <= 113)
                    {
                        if (i == 112)
                            return 2733636028;
                        return 2753990216;
                    }

                    if (i == 114)
                        return 2774239903;
                    return 2794384321;
                }

                if (i <= 117)
                {
                    if (i == 116)
                        return 2814422705;
                    return 2834354295;
                }

                if (i == 118)
                    return 2854178334;
                return 2873894071;
            }

            if (i <= 123)
            {
                if (i <= 121)
                {
                    if (i == 120)
                        return 2893500756;
                    return 2912997648;
                }

                if (i == 122)
                    return 2932384004;
                return 2951659090;
            }

            if (i <= 125)
            {
                if (i == 124)
                    return 2970822175;
                return 2989872531;
            }

            if (i == 126)
                return 3008809435;
            return 3027632170;
        }

        if (i <= 191)
        {
            if (i <= 159)
            {
                if (i <= 143)
                {
                    if (i <= 135)
                    {
                        if (i <= 131)
                        {
                            if (i <= 129)
                            {
                                if (i == 128)
                                    return 3046340019;
                                else
                                    return 3064932275;
                            }
                            else
                            {
                                if (i == 130)
                                    return 3083408230;
                                else
                                    return 3101767185;
                            }
                        }
                        else
                        {
                            if (i <= 133)
                            {
                                if (i == 132)
                                    return 3120008443;
                                else
                                    return 3138131310;
                            }
                            else
                            {
                                if (i == 134)
                                    return 3156135101;
                                else
                                    return 3174019130;
                            }
                        }
                    }

                    if (i <= 139)
                    {
                        if (i <= 137)
                        {
                            if (i == 136)
                                return 3191782721;
                            else
                                return 3209425199;
                        }
                        else
                        {
                            if (i == 138)
                                return 3226945894;
                            else
                                return 3244344141;
                        }
                    }
                    else
                    {
                        if (i <= 141)
                        {
                            if (i == 140)
                                return 3261619281;
                            else
                                return 3278770658;
                        }
                        else
                        {
                            if (i == 142)
                                return 3295797620;
                            else
                                return 3312699523;
                        }
                    }
                }

                if (i <= 151)
                {
                    if (i <= 147)
                    {
                        if (i <= 145)
                        {
                            if (i == 144)
                                return 3329475725;
                            else
                                return 3346125588;
                        }
                        else
                        {
                            if (i == 146)
                                return 3362648482;
                            else
                                return 3379043779;
                        }
                    }
                    else
                    {
                        if (i <= 149)
                        {
                            if (i == 148)
                                return 3395310857;
                            else
                                return 3411449099;
                        }
                        else
                        {
                            if (i == 150)
                                return 3427457892;
                            else
                                return 3443336630;
                        }
                    }
                }
                else
                {
                    if (i <= 155)
                    {
                        if (i <= 153)
                        {
                            if (i == 152)
                                return 3459084709;
                            else
                                return 3474701532;
                        }
                        else
                        {
                            if (i == 154)
                                return 3490186507;
                            else
                                return 3505539045;
                        }
                    }
                    else
                    {
                        if (i <= 157)
                        {
                            if (i == 156)
                                return 3520758565;
                            else
                                return 3535844488;
                        }
                        else
                        {
                            if (i == 158)
                                return 3550796243;
                            else
                                return 3565613262;
                        }
                    }
                }
            }

            if (i <= 175)
            {
                if (i <= 167)
                {
                    if (i <= 163)
                    {
                        if (i <= 161)
                        {
                            if (i == 160)
                                return 3580294982;
                            else
                                return 3594840847;
                        }
                        else
                        {
                            if (i == 162)
                                return 3609250305;
                            else
                                return 3623522808;
                        }
                    }
                    else
                    {
                        if (i <= 165)
                        {
                            if (i == 164)
                                return 3637657816;
                            else
                                return 3651654792;
                        }
                        else
                        {
                            if (i == 166)
                                return 3665513205;
                            else
                                return 3679232528;
                        }
                    }
                }
                else
                {
                    if (i <= 171)
                    {
                        if (i <= 169)
                        {
                            if (i == 168)
                                return 3692812243;
                            else
                                return 3706251832;
                        }
                        else
                        {
                            if (i == 170)
                                return 3719550786;
                            else
                                return 3732708601;
                        }
                    }
                    else
                    {
                        if (i <= 173)
                        {
                            if (i == 172)
                                return 3745724777;
                            else
                                return 3758598821;
                        }
                        else
                        {
                            if (i == 174)
                                return 3771330243;
                            else
                                return 3783918561;
                        }
                    }
                }
            }
            else
            {
                if (i <= 183)
                {
                    if (i <= 179)
                    {
                        if (i <= 177)
                        {
                            if (i == 176)
                                return 3796363297;
                            else
                                return 3808663979;
                        }
                        else
                        {
                            if (i == 178)
                                return 3820820141;
                            else
                                return 3832831319;
                        }
                    }
                    else
                    {
                        if (i <= 181)
                        {
                            if (i == 180)
                                return 3844697060;
                            else
                                return 3856416913;
                        }
                        else
                        {
                            if (i == 182)
                                return 3867990433;
                            else
                                return 3879417181;
                        }
                    }
                }
                else
                {
                    if (i <= 187)
                    {
                        if (i <= 185)
                        {
                            if (i == 184)
                                return 3890696723;
                            else
                                return 3901828632;
                        }
                        else
                        {
                            if (i == 186)
                                return 3912812484;
                            else
                                return 3923647863;
                        }
                    }
                    else
                    {
                        if (i <= 189)
                        {
                            if (i == 188)
                                return 3934334359;
                            else
                                return 3944871565;
                        }
                        else
                        {
                            if (i == 190)
                                return 3955259082;
                            else
                                return 3965496515;
                        }
                    }
                }
            }
        }

        if (i <= 223)
        {
            if (i <= 207)
            {
                if (i <= 199)
                {
                    if (i <= 195)
                    {
                        if (i <= 193)
                        {
                            if (i == 192)
                                return 3975583476;
                            else
                                return 3985519583;
                        }
                        else
                        {
                            if (i == 194)
                                return 3995304457;
                            else
                                return 4004937729;
                        }
                    }
                    else
                    {
                        if (i <= 197)
                        {
                            if (i == 196)
                                return 4014419032;
                            else
                                return 4023748007;
                        }
                        else
                        {
                            if (i == 198)
                                return 4032924300;
                            else
                                return 4041947562;
                        }
                    }
                }
                else
                {
                    if (i <= 203)
                    {
                        if (i <= 201)
                        {
                            if (i == 200)
                                return 4050817451;
                            else
                                return 4059533630;
                        }
                        else
                        {
                            if (i == 202)
                                return 4068095769;
                            else
                                return 4076503544;
                        }
                    }
                    else
                    {
                        if (i <= 205)
                        {
                            if (i == 204)
                                return 4084756634;
                            else
                                return 4092854726;
                        }
                        else
                        {
                            if (i == 206)
                                return 4100797514;
                            else
                                return 4108584696;
                        }
                    }
                }
            }
            else
            {
                if (i <= 215)
                {
                    if (i <= 211)
                    {
                        if (i <= 209)
                        {
                            if (i == 208)
                                return 4116215977;
                            else
                                return 4123691067;
                        }
                        else
                        {
                            if (i == 210)
                                return 4131009681;
                            else
                                return 4138171544;
                        }
                    }
                    else
                    {
                        if (i <= 213)
                        {
                            if (i == 212)
                                return 4145176382;
                            else
                                return 4152023930;
                        }
                        else
                        {
                            if (i == 214)
                                return 4158713929;
                            else
                                return 4165246124;
                        }
                    }
                }
                else
                {
                    if (i <= 219)
                    {
                        if (i <= 217)
                        {
                            if (i == 216)
                                return 4171620267;
                            else
                                return 4177836117;
                        }
                        else
                        {
                            if (i == 218)
                                return 4183893437;
                            else
                                return 4189791999;
                        }
                    }
                    else
                    {
                        if (i <= 221)
                        {
                            if (i == 220)
                                return 4195531577;
                            else
                                return 4201111955;
                        }
                        else
                        {
                            if (i == 222)
                                return 4206532921;
                            else
                                return 4211794268;
                        }
                    }
                }
            }
        }
        else
        {
            if (i <= 239)
            {
                if (i <= 231)
                {
                    if (i <= 227)
                    {
                        if (i <= 225)
                        {
                            if (i == 224)
                                return 4216895797;
                            else
                                return 4221837315;
                        }
                        else
                        {
                            if (i == 226)
                                return 4226618635;
                            else
                                return 4231239573;
                        }
                    }
                    else
                    {
                        if (i <= 229)
                        {
                            if (i == 228)
                                return 4235699957;
                            else
                                return 4239999615;
                        }
                        else
                        {
                            if (i == 230)
                                return 4244138385;
                            else
                                return 4248116110;
                        }
                    }
                }
                else
                {
                    if (i <= 235)
                    {
                        if (i <= 233)
                        {
                            if (i == 232)
                                return 4251932639;
                            else
                                return 4255587827;
                        }
                        else
                        {
                            if (i == 234)
                                return 4259081536;
                            else
                                return 4262413632;
                        }
                    }
                    else
                    {
                        if (i <= 237)
                        {
                            if (i == 236)
                                return 4265583990;
                            else
                                return 4268592489;
                        }
                        else
                        {
                            if (i == 238)
                                return 4271439015;
                            else
                                return 4274123460;
                        }
                    }
                }
            }
            else
            {
                if (i <= 247)
                {
                    if (i <= 243)
                    {
                        if (i <= 241)
                        {
                            if (i == 240)
                                return 4276645722;
                            else
                                return 4279005706;
                        }
                        else
                        {
                            if (i == 242)
                                return 4281203321;
                            else
                                return 4283238485;
                        }
                    }
                    else
                    {
                        if (i <= 245)
                        {
                            if (i == 244)
                                return 4285111119;
                            else
                                return 4286821154;
                        }
                        else
                        {
                            if (i == 246)
                                return 4288368525;
                            else
                                return 4289753172;
                        }
                    }
                }
                else
                {
                    if (i <= 251)
                    {
                        if (i <= 249)
                        {
                            if (i == 248)
                                return 4290975043;
                            else
                                return 4292034091;
                        }
                        else
                        {
                            if (i == 250)
                                return 4292930277;
                            else
                                return 4293663567;
                        }
                    }
                    else
                    {
                        if (i <= 253)
                        {
                            if (i == 252)
                                return 4294233932;
                            else
                                return 4294641351;
                        }
                        else
                        {
                            if (i == 254)
                                return 4294885809;
                            else
                                return 4294967296;
                        }
                    }
                }
            }
        }
    }
}