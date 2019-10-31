using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using Microsoft.VisualBasic.Devices;
using System.Threading;

namespace ComPort
{

    public partial class Form1 : Form
    {
        string SCA, TPDU, PDUType, TPmr, TPda, TPpid, TPdcs, TPvp, TPudl, TPud;
        int smslength;
        string SMS;
        SerialPort comport;

        delegate void SetTextCallback(string text);

        private Thread demoThread = null;


        public Form1()
        {
            InitializeComponent();
        }


        private void DoCommand(string command)
        {
            try
            {
                if (comport != null)
                {
                    if (comport.IsOpen)
                    {
                        comport.WriteLine(command);
                    }
                    else
                    {
                        MessageBox.Show("Port Is Closed! At First Open Port", "Error in Command");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }


        private void CLOSEPORT()
        {
            try
            {
                comport.Close();
                comport = null;
                MessageBox.Show("Модем успешно отключен", "Отключено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                tlstripStatus.Text = "ОТКЛЮЧЕН";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }


        private string EncodePhoneNumber(string PhoneNumber)
        {
            string result = "";

            PhoneNumber = PhoneNumber.Replace("+", "");
            if ((PhoneNumber.Length % 2) > 0)
                PhoneNumber += "F";
            int i = 0;
            while (i < PhoneNumber.Length)
            {
                result += PhoneNumber[i + 1].ToString() + PhoneNumber[i].ToString();
                i += 2;
            }
            return result.Trim();
        }


        private SerialPort SetPort(string comname)
        {
            if (comport != null)
                comport.Dispose();
            SerialPort port = new SerialPort();
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.PortName = comname;
            port.WriteTimeout = 5000;
            port.ReadTimeout = 5000;
            port.BaudRate = 9600;
            port.Parity = Parity.None;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.RequestToSend;

            port.DtrEnable = true;
            port.RtsEnable = true;
            port.NewLine = System.Environment.NewLine;
            try
            {
                port.Open();
                MessageBox.Show("Модем успешно подключен", "Подключено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                tlstripStatus.Text = "ПОДКЛЮЧЕН";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                port.Close();
                tlstripStatus.Text = "ОТКЛЮЧЕН";
                return null;
            }
            return port;
        }

        string ss, mem, tt, RcvdPDU, msg = "";
        int index;

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(300);
                tt += comport.ReadExisting();
                ss = tt;
                string[] split = tt.Split(Convert.ToChar(10));
                for (int i = 0; i < split.Length; i++)
                {
                    tt = split[i];
                    if (split[i].StartsWith("+CMTI:"))
                    {
                        tt = split[i] + ",";
                        tt = tt.Substring(6);
                        string[] tmp = tt.Split(',');
                        mem = tmp[0];
                        mem = mem.ToLower();
                        index = Convert.ToInt32(tmp[1]);

                        index = Convert.ToInt32(tmp[1]);
                        comport.WriteLine("AT+CPMS=" + mem + "\r");
                        Thread.Sleep(300);
                        tt = comport.ReadExisting();
                        comport.WriteLine("AT+CMGR=" + index.ToString() + "\r");
                        Thread.Sleep(500);
                        tt = comport.ReadExisting();
                        string[] tmp1 = tt.Split(Convert.ToChar(10));
                        for (int jjj = 0; jjj < tmp1.Length; jjj++)
                        {
                            tt = tmp1[jjj];
                            if (tt.StartsWith("+CMGR:"))
                            {
                                RcvdPDU = tmp1[jjj + 1];
                                PduDecoder decode = new PduDecoder();
                                msg = decode.GetPdu(RcvdPDU);
                                string mSa = decode.SMSCAddress;
                                string tOm = decode.SMSType;
                                string pid = decode.ProtocolID;
                                string sndrNo = decode.SenderMobileno;
                                string DcS = decode.DataCodingScheme;
                                string time = decode.TimeStamp;

                            }
                        }
                    }
                    else
                    {
                        if (split[i].StartsWith("AT+CGMI"))
                        {
                            if (split.Length > 2)
                            {
                                MessageBox.Show(split[1].ToString());
                            }

                        }

                 

                  
                        else
                        {
                            if (split[i].StartsWith("+CMGR:"))
                            {
                                RcvdPDU = split[2];
                                PduDecoder decode = new PduDecoder();
                                msg = decode.GetPdu(RcvdPDU);
                                string mSa = decode.SMSCAddress;
                                string tOm = decode.SMSType;
                                string pid = decode.ProtocolID;
                                string sndrNo = decode.SenderMobileno;
                                string DcS = decode.DataCodingScheme;
                                string time = decode.TimeStamp;
                                string message = decode.Message;
                                MessageBox.Show("Message From " + sndrNo + Environment.NewLine + " Meassage body \n" + message + Environment.NewLine + " Time Created " + time);
                            }
                            else
                            {
                                if (split[i].StartsWith("+CMGS:"))
                                {
                                    MessageBox.Show("Отчет о доставке: Сообщение доставлено!");
                                }
                            }
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
                comport.Close();
            }

            //Application.DoEvents();
            this.demoThread =
                new Thread(new ThreadStart(this.ThreadProcSafe));
            this.demoThread.Start();
            tt = "";
        }

        private void ThreadProcSafe()
        {
            this.SetText(ss);
        }

        private void SetText(string text)
        {
            if (this.txtAnswer.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.txtAnswer.Text += text;
                ss = "";
                demoThread.Abort();
            }
        }

        private void SendSms()
        {

            PDUType = "01";
            TPmr = "00";
            TPda = txtPhoneNumber.Text.Length.ToString("X2") + "91" + EncodePhoneNumber(txtPhoneNumber.Text); //nomer poluchatelya
           // TPpid = "00";
            if (chkFlash.Checked)
            {
                if (!IsLatin(txtSmsBody.Text))
                {
                    TPdcs = "10";
                }
                else
                {
                    TPdcs = "18";
                }
            }

                
            else
            {
                if (!IsLatin(txtSmsBody.Text))
                {
                    TPdcs = "00";
                }
                else
                {
                    TPdcs = "08";
                }
                if (SilentCh.Checked)
                {
                    TPpid = "40";

                }
                else
                {
                    TPpid = "00";

                }

            }
            TPvp = "";
            if (IsLatin(txtSmsBody.Text))
            {
                TPudl = (txtSmsBody.Text.Length * 2).ToString("X2");
            }
            else
            {
                TPudl = txtSmsBody.Text.Length.ToString("X2");
            }
            if (!IsLatin(txtSmsBody.Text))
            {
                TPud = String7To8(txtSmsBody.Text);
            }
            else
            {
                TPud = StringToUCS2(txtSmsBody.Text);
            }
            TPDU = PDUType + TPmr + TPda + TPpid + TPdcs + TPvp + TPudl + TPud;

            smslength = TPDU.Length / 2;
            string centernumber = EncodePhoneNumber(txtSMSCenter.Text);
            SCA = ((centernumber.Length / 2) + 1).ToString("X2") + "91" + centernumber;

            SMS = SCA + TPDU;


            try
            {
                comport.WriteLine("AT+CMGS=" + smslength);

                Thread.Sleep(3000);

                comport.WriteLine(SMS + (char)(26));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

       

        private void btnSmsSend_Click(object sender, EventArgs e)
        {
            if (comport != null)
                if (comport.IsOpen)
                {
                    SendSms();
                }
                else
                {
                    MessageBox.Show("At First open port");
                }
            //rec to file l0g
            string star = "************************************************\n";

            string date = "Дата/время отправления -> " + DateTime.Now.ToString("dd MMMM yyyy | HH:mm:ss") + "\n";
            string UserPhone = "Номер получателя -> " + txtPhoneNumber.Text + "\n";
            string SMScenter = "СМС Центр -> " + txtSMSCenter.Text + "\n";
            string TextSms = "Текст СМС -> " + txtSmsBody.Text + "\n";
            string sms_stat = "Тип СМС -> Обычная\n";
            string dash = "------------------------------------------------\n";

            if (chkFlash.Checked)
            {
                
                
                string FlashSms = "Тип СМС -> Флэш смс\n";
                string l0g_flash = star + date + UserPhone + SMScenter + TextSms + FlashSms + dash;
                System.IO.StreamWriter writerr = new System.IO.StreamWriter("l0g.ph", true);
                writerr.WriteLine(l0g_flash);
                writerr.Close();
            }
           // else {chkFlash.Enabled = false;}
             if (SilentCh.Checked)
            {


                string SilentSms = "Тип СМС -> Тихая смс\n";
                string l0g_silent = star+date + UserPhone + SMScenter + TextSms + SilentSms + dash;
                System.IO.StreamWriter writerrr = new System.IO.StreamWriter("l0g.ph", true);
                writerrr.WriteLine(l0g_silent);
                writerrr.Close();



            }

            else
            {
                string l0g = star + date + UserPhone + SMScenter + TextSms + sms_stat + dash;
                // System.IO.File.WriteAllText(@"l0g.ph", l0g);
                System.IO.StreamWriter writer = new System.IO.StreamWriter("l0g.ph", true);
                writer.WriteLine(l0g);
                writer.Close();

            }

           
     
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Computer cmp = new Computer();
            List<string> localports = new List<string>();
            localports.AddRange(cmp.Ports.SerialPortNames);
            foreach (string ss in localports)
            {
                cmbPorts.Items.Add(ss);
            }
            try
            {
                txtSMSCenter.Text = File.ReadAllText("cn.key");
            }
            catch { }
        }



        private void btnOpen_Click(object sender, EventArgs e)
        {

            if (comport == null && cmbPorts.SelectedIndex > -1)
            {
                comport = SetPort(cmbPorts.SelectedItem.ToString());
            }

        }


        private bool IsLatin(string s)
        {
            if (Regex.Match(s, "[^\x00-\x80]").Success)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        private void txtAnswer_TextChanged(object sender, EventArgs e)
        {
            txtAnswer.SelectionStart = txtAnswer.Text.Length;
            txtAnswer.ScrollToCaret();
        }

        private void btnCLOSEPORT_Click(object sender, EventArgs e)
        {
            CLOSEPORT();
        }

        private void txtSmsBody_TextChanged(object sender, EventArgs e)
        {
            if (IsLatin(txtSmsBody.Text))
            {
                txtSmsBody.MaxLength = 70;

            }
            else
            {
                txtSmsBody.MaxLength = 140;
            }
            lblSymbols.Text = "Символы - " + txtSmsBody.Text.Length;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText("cn.key", txtSMSCenter.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DoCommand(textBox1.Text);
        }

        private void btnModemName_Click(object sender, EventArgs e)
        {
            DoCommand("AT+CGMI");

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
          
        }

     

        private void chkFlash_CheckedChanged(object sender, EventArgs e)
        {

        }

       
        public string StringToUCS2(string str)
        {
            UnicodeEncoding ue = new UnicodeEncoding();
            byte[] ucs2 = ue.GetBytes(str);
            int i = 0;
            while (i < ucs2.Length)
            {
                byte b = ucs2[i + 1];
                ucs2[i + 1] = ucs2[i];
                ucs2[i] = b;
                i += 2;
            }
            return BitConverter.ToString(ucs2).Replace("-", "");
        }

        public string String7To8(string str)
        {
            string result = "";
            ASCIIEncoding ae = new ASCIIEncoding();
            byte[] arr = ae.GetBytes(str);
            int i = 1;
            while (i < arr.Length)
            {
                int j = arr.Length - 1;
                while (j >= i)
                {
                    byte firstBit = (arr[j] % 2 > 0) ? (byte)0x80 : (byte)0x00;
                    arr[j - 1] = (byte)((arr[j - 1] & 0x7f) | firstBit);
                    arr[j] = (byte)(arr[j] >> 1);
                    j--;
                }
                i++;
            }
            i = 0;
            while ((i < arr.Length) && (arr[i] != 0))
            {
                result += arr[i].ToString("X2");
                i++;
            }
            return result.Trim();
        }

  
        }

    }
