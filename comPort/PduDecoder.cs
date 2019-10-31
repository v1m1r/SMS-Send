using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace ComPort
{

    class PduDecoder
    {
        List<string> addertable = new List<string>();
        List<string> basetable = new List<string>();
        List<string> Mergedseptet = new List<string>();
        List<string> HextoOctet = new List<string>();
        int smsclen = 0;
        string temp = "";
        string typeofaddr = "";
        string smscaddr = "";
        string SMStype = "";
        string smsdeliver = "";
        int sendernolen = 0;
        string senderno = "";
        string TP_PID = "";
        string TP_DCS = "";
        string TP_SCTS = "";
        string TP_UDL = "";
        string TP_UD = "";
        int msglen = 0;
        string userPDU = "";
        string _msg = "";
        public string SMSCAddress
        {
            get { return smscaddr; }
        }

        public string SMSType
        {
            get { return typeofaddr; }
        }

        public string SenderMobileno
        {
            get { return senderno; }
        }
        public string ProtocolID
        {
            get { return TP_PID; }
        }
        public string DataCodingScheme
        {
            get { return TP_DCS; }
        }

        public string TimeStamp
        {
            get { return TP_SCTS; }
        }
        public string Message
        {
            get { return _msg; }
        }


        public string GetPdu(string PDU)
        {

            PDU = PDU.TrimEnd(Convert.ToChar(13));
            smsclen = Convert.ToInt32(PDU.Substring(0, 2));//Length of septets
            PDU = PDU.Substring(2);
            smsclen = smsclen * 2;


            smscaddr = PDU.Substring(0, smsclen);
            PDU = PDU.Substring(smsclen);
            typeofaddr = smscaddr.Substring(0, 2);
            smscaddr = smscaddr.Substring(2);
            smscaddr = DecodePhoneNumber(smscaddr);

            typeofaddr = PDU.Substring(0, 2);
            PDU = PDU.Substring(2);

            temp = PDU.Substring(0, 2);
            PDU = PDU.Substring(2);
            sendernolen = Convert.ToInt32(temp, 16);

            PDU = PDU.Substring(2);
            if (PDU.Substring(0, sendernolen).EndsWith("F"))
            {
                senderno = DecodePhoneNumber(PDU.Substring(0, sendernolen + 1));
                PDU = PDU.Substring(sendernolen + 1);
            }
            else
            {
                senderno = DecodePhoneNumber(PDU.Substring(0, sendernolen));
                PDU = PDU.Substring(sendernolen);
            }

            TP_PID = PDU.Substring(0, 2);
            PDU = PDU.Substring(2);

            TP_DCS = PDU.Substring(0, 2);
            PDU = PDU.Substring(2);

            TP_SCTS = PDU.Substring(0, 14);
            PDU = PDU.Substring(14);
            TP_SCTS = DecodePhoneNumber(TP_SCTS);
            TP_SCTS = getTimedate(TP_SCTS);

            temp = "";
            temp = PDU.Substring(0, 2);
            msglen = Convert.ToInt32(temp, 16);
            msglen = (msglen * 2);

            userPDU = PDU;
            userPDU = userPDU.Substring(2);

            Thread.Sleep(500);
            HexToOctet(userPDU, msglen);
            Thread.Sleep(500);
            _msg = OctetToString();
            return _msg;

        }

        private string DecodePhoneNumber(string PhoneNumber)
        {
            string result = "";
            int i = 0;
            while (i < PhoneNumber.Length)
            {
                result += PhoneNumber[i + 1].ToString() + PhoneNumber[i].ToString();
                i += 2;
            }

            result = result.Replace("F", "");

            return result.Trim();
        }

        private string getTimedate(string Timdat)
        {
            string year, month, day, hour, min, sec, date_time;
            year = Timdat.Substring(0, 2);
            Timdat = Timdat.Substring(2);
            month = Timdat.Substring(0, 2);
            Timdat = Timdat.Substring(2);
            day = Timdat.Substring(0, 2);
            Timdat = Timdat.Substring(2);
            hour = Timdat.Substring(0, 2);
            Timdat = Timdat.Substring(2);
            min = Timdat.Substring(0, 2);
            Timdat = Timdat.Substring(2);
            sec = Timdat.Substring(0, 2);
            date_time = day + "/" + month + "/" + year + " " + hour + ":" + min + ":" + sec;
            return date_time;
        }

        private void HexToOctet(string O2S, int msglen)
        {
            //I know that this logic is an old school one but i'ad no alternatives 
            try
            {
                int loop = 0;
                while (loop < (msglen - 1))
                {
                    string Bin1 = "";
                    string Bin2 = "";
                    string FHex = O2S.Substring(loop, 1);// Taking the hex value 1 by 1
                    string SHex = O2S.Substring(loop + 1, 1);//= 
                    string octet = "";
                    switch (FHex)
                    {
                        case "0": Bin1 = "0000"; break;
                        case "1": Bin1 = "0001"; break;
                        case "2": Bin1 = "0010"; break;
                        case "3": Bin1 = "0011"; break;
                        case "4": Bin1 = "0100"; break;
                        case "5": Bin1 = "0101"; break;
                        case "6": Bin1 = "0110"; break;
                        case "7": Bin1 = "0111"; break;
                        case "8": Bin1 = "1000"; break;
                        case "9": Bin1 = "1001"; break;
                        case "A": Bin1 = "1010"; break;
                        case "B": Bin1 = "1011"; break;
                        case "C": Bin1 = "1100"; break;
                        case "D": Bin1 = "1101"; break;
                        case "E": Bin1 = "1110"; break;
                        case "F": Bin1 = "1111"; break;
                        default: break;
                    }
                    //loop = loop + 1;
                    switch (SHex)
                    {
                        case "0": Bin2 = "0000"; break;
                        case "1": Bin2 = "0001"; break;
                        case "2": Bin2 = "0010"; break;
                        case "3": Bin2 = "0011"; break;
                        case "4": Bin2 = "0100"; break;
                        case "5": Bin2 = "0101"; break;
                        case "6": Bin2 = "0110"; break;
                        case "7": Bin2 = "0111"; break;
                        case "8": Bin2 = "1000"; break;
                        case "9": Bin2 = "1001"; break;
                        case "A": Bin2 = "1010"; break;
                        case "B": Bin2 = "1011"; break;
                        case "C": Bin2 = "1100"; break;
                        case "D": Bin2 = "1101"; break;
                        case "E": Bin2 = "1110"; break;
                        case "F": Bin2 = "1111"; break;
                        default: break;
                    }

                    loop = loop + 2;
                    octet = Bin1 + Bin2;
                    HextoOctet.Add(octet);
                }

            }
            catch (Exception ex)
            {
                string ss = ex.Message.ToString();

            }


            //PDULen=PDULen.ToString(


        }

        private string OctetToString()
        {
            int adderbitcnt = 1;

            string first = "";
            string adder = "";


            int arrayindex = 0;
            #region Adder Table
            try
            {
                for (int loop = 0; loop <= HextoOctet.Count; loop++)
                {
                    if (loop == 0)
                    {
                        addertable.Add("0");
                        first = HextoOctet[0].Substring(1, 7);
                        //basetable.Add(first);
                    }
                    else
                    {
                        if (adderbitcnt == 8)
                        {
                            addertable.Add("0");
                            adderbitcnt = 1;
                        }
                        else
                        {
                            adder = HextoOctet[arrayindex].Substring(0, adderbitcnt);
                            addertable.Add(adder);
                            adderbitcnt++;
                            arrayindex++;
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message.ToString();

                //MessageBox.Show("Adder table exception caught");
            }
            #endregion

            #region basetable
            int arrayindex1 = 0;
            int startbase = 1;
            int basebitcount = 7;
            string baseinfo = "";
            try
            {
                for (int loop1 = 0; loop1 <= HextoOctet.Count; loop1++)
                {
                    if (startbase == 8)
                    {
                        startbase = 1;
                        basetable.Add("0");
                    }
                    else
                    {
                        baseinfo = "";
                        basebitcount = 8 - startbase;
                        baseinfo = HextoOctet[arrayindex1].Substring(startbase, basebitcount);
                        baseinfo.Trim();
                        basetable.Add(baseinfo);
                        //basebitcount--;
                        startbase++;
                        arrayindex1++;
                    }
                    //}
                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message.ToString();

                //MessageBox.Show("Base table exception"); 
            }
            #endregion



            #region Merging AdderTable&BaseTable
            //string addtblInfo = "";
            //string basetblInfo = "";
            string DECSeptet = "";
            try
            {
                for (int merge = 0; merge < basetable.Count; merge++)
                {
                    if (addertable[merge] == "0")
                    {
                        DECSeptet = basetable[merge];
                        Mergedseptet.Add(DECSeptet);
                    }
                    else
                    {
                        DECSeptet = "0" + basetable[merge] + addertable[merge];
                        Mergedseptet.Add(DECSeptet);
                    }
                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message.ToString();

                //MessageBox.Show("Merging Exceptiion"); 
            }
            #endregion


            #region DECODEDTOSTRING
            string success = "";
            try
            {
                for (int decode = 0; decode < Mergedseptet.Count; decode++)
                {
                    long l = Convert.ToInt64(Mergedseptet[decode], 2);
                    int i = (int)l;
                    //--------------New Area
                    if (i == 26 || i == 28)
                    {
                        if (i == 26)
                        {
                            success += "4";
                        }
                        if (i == 28)
                        {
                            success += "8";
                        }
                    }
                    //New area
                    else
                    {
                        success += Convert.ToChar(i);
                    }

                }


                //txtMessage.Text = " DECODED MESSAGE =" + success;
            }
            catch (Exception ex)
            {
                string ss = ex.Message.ToString();
                //MessageBox.Show("Decode exception caught"); 
            }


            //OctettoSeptet.Clear();
            #endregion


            HextoOctet.Clear();
            addertable.Clear();
            basetable.Clear();
            Mergedseptet.Clear();
            return success;



        }
        private void decode()
        {
            int smsclength, tpdalength = 0;
            int tpudl, ind = 0;
            string smsc, smscformat, pdutype, tpda, tpdaformat, tppid, tpdcs, timestamp, gsmzone, tpud;
            string allpdu = "";
            smsclength = int.Parse(allpdu.Substring(0, 2));

            if (smsclength > 0)
            {
                smscformat = allpdu.Substring(2, 2);
                smsc = DecodePhoneNumber(allpdu.Substring(4, smsclength * 2 - 2));
                ind = smsclength * 2 + 2;
            }
            else
            {
                smsc = "00";
                smsclength = 2;
                ind = 2 * smsclength;
            }

            pdutype = allpdu.Substring(ind, 2);
            ind += 2;
            tpdalength = Convert.ToInt32(allpdu.Substring(ind, 2), 16);

            ind += 2;
            tpdaformat = allpdu.Substring(ind, 2);
            ind += 2;
            if (allpdu.Substring(ind, tpdalength).EndsWith("F"))
            {
                tpda = DecodePhoneNumber(allpdu.Substring(ind, tpdalength + 1));
                ind += 1;
            }
            else
            {
                tpda = DecodePhoneNumber(allpdu.Substring(ind, tpdalength));
            }
            ind += tpdalength;
            tppid = allpdu.Substring(ind, 2);
            ind += 2;
            tpdcs = allpdu.Substring(ind, 2);
            ind += 2;
            timestamp = getTimedate(DecodePhoneNumber(allpdu.Substring(ind, 12)));
            ind += 12;
            gsmzone = allpdu.Substring(ind, 2);
            ind += 2;
            tpudl = Convert.ToInt32(allpdu.Substring(ind, 2), 16);
            ind += 2;
            tpud = allpdu.Substring(ind);
            HexToOctet(tpud, tpudl * 2);
            string message = OctetToString();
            //MessageBox.Show(message, timestamp);
        }
    }
}
