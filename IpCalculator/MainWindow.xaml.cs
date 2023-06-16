using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace IpCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public int IpAddress1 { get; set; }
        public int IpAddress2 { get; set; }
        public int IpAddress3 { get; set; }
        public int IpAddress4 { get; set; }
        public int Subnetzmaske1 { get; set; }
        public int Subnetzmaske2 { get; set; }
        public int Subnetzmaske3 { get; set; }
        public int Subnetzmaske4 { get; set; }

        private void CheckInputType(object sender, TextCompositionEventArgs e)
        {
            //Da in der Eingabebox auch Buchstaben eingegeben werden können, musste ich erstmal die Eingabe auf Ziffern begrenzen.
            //Dazu verwendete ich eine Regex abfrage.
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void Button_Berechnen_Click(object sender, RoutedEventArgs e)
        {
            // Da meine eingabe aus 4 eingabefelder besteht wird nur fortgesetzt wenn alle 4Textboxen einen Wert haben.
            if (!string.IsNullOrEmpty(InputIpAddress1.Text)
                && !string.IsNullOrEmpty(InputIpAddress2.Text)
                && !string.IsNullOrEmpty(InputIpAddress3.Text)
                && !string.IsNullOrEmpty(InputIpAddress4.Text))
            {
                //Der Wert wird von einem String in einen Integer umgewandelt.
                IpAddress1 = Convert.ToInt32(InputIpAddress1.Text);
                IpAddress2 = Convert.ToInt32(InputIpAddress2.Text);
                IpAddress3 = Convert.ToInt32(InputIpAddress3.Text);
                IpAddress4 = Convert.ToInt32(InputIpAddress4.Text);
            }

            if (!string.IsNullOrEmpty(InputSubnetzmask1.Text)
                && !string.IsNullOrEmpty(InputSubnetzmask2.Text)
                && !string.IsNullOrEmpty(InputSubnetzmask3.Text)
                && !string.IsNullOrEmpty(InputSubnetzmask4.Text))
            {
                //Der Wert wird von einem String in einen Integer umgewandelt.
                Subnetzmaske1 = Convert.ToInt32(InputSubnetzmask1.Text);
                Subnetzmaske2 = Convert.ToInt32(InputSubnetzmask2.Text);
                Subnetzmaske3 = Convert.ToInt32(InputSubnetzmask3.Text);
                Subnetzmaske4 = Convert.ToInt32(InputSubnetzmask4.Text);
            }

            //Ausgabe der Adresse
            var ipAddress = DisplayAddress(IpAddress1, IpAddress2, IpAddress3, IpAddress4);

            //Umwandeln der einzelnen Werte in eine Adresse.
            var subnet = GetSubnetFromInt(Subnetzmaske1, Subnetzmaske2, Subnetzmaske3, Subnetzmaske4);

            //Ausgabe der Broadcastadresse
            var broadcast = DisplayBroadcastAddress(ipAddress, subnet);

            //Ausgabe der Broadcastadresse
            DisplayNetworkId(ipAddress, subnet);

            //Ausgabe der Broadcastadresse
            var firstHost = DisplayFirstHost(ipAddress, subnet);

            //Ausgabe der Broadcastadresse
            var lastHost = DisplayLastHost(broadcast, subnet);

            //Ausgabe der Broadcastadresse
            DisplayHostsNetz(firstHost, lastHost);
        }

        private IPAddress DisplayAddress(int ipAddress1, int ipAddress2, int ipAddress3, int ipAddress4)
        {
            //Die einzelnen Werte werden zu einer Ip-Adresse umgewandelt.
            var ipAddress = new IPAddress(new[]
            {
                Convert.ToByte(ipAddress1),
                Convert.ToByte(ipAddress2),
                Convert.ToByte(ipAddress3),
                Convert.ToByte(ipAddress4)
            });

            //Die nun bestehende Ip-Adresse wird im Frontend ausgegben.
            OutputAddress.Content = ipAddress;

            return ipAddress;
        }

        private IPAddress GetSubnetFromInt(int subnetzmaske1, int subnetzmaske2, int subnetzmaske3, int subnetzmaske4)
        {
            //Die einzelnen Werte werden zu einer Ip-Adresse umgewandelt.
            return new IPAddress(new[]
            {
                Convert.ToByte(subnetzmaske1),
                Convert.ToByte(subnetzmaske2),
                Convert.ToByte(subnetzmaske3),
                Convert.ToByte(subnetzmaske4)
            });
        }

        public IPAddress DisplayBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            //Hier wird mittels der IpAdresse und der Subnetmakse die Broadcastadresse erstellt.
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Länge der IP-Adresse und der Subnetzmaske passen nicht überein.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));

            OutputBroadcast.Content = new IPAddress(broadcastAddress);

            return new IPAddress(broadcastAddress);
        }

        public void DisplayNetworkId(IPAddress address, IPAddress subnetMask)
        {
            //Hier wird mittels der IpAdresse und der Subnetmakse die NetzwerkId erstellt.
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Länge der IP-Adresse und der Subnetzmaske passen nicht überein.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & subnetMaskBytes[i]);

            OutputNetworkId.Content = new IPAddress(broadcastAddress);
        }

        private IPAddress DisplayFirstHost(IPAddress ipAddress, IPAddress subnet)
        {
            var firstIp = (ConvertFromIpAddressToInteger(ipAddress) & ConvertFromIpAddressToInteger(subnet)) + 1;

            var result = OutputFirstHost.Content = ConvertFromIntegerToIpAddress(firstIp);

            return IPAddress.Parse(result.ToString());
        }

        public int ConvertFromIpAddressToInteger(IPAddress ipAddress)
        {
            var address = IPAddress.Parse(ipAddress.ToString());
            var bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public string ConvertFromIntegerToIpAddress(int ipAddress)
        {
            var bytes = BitConverter.GetBytes(ipAddress);

            // flip little-endian to big-endian(network order)
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

            return new IPAddress(bytes).ToString();
        }

        private IPAddress DisplayLastHost(IPAddress address, IPAddress subnetMask)
        {
            var splitedaddress = address.ToString().Split(".");

            var newlast = Convert.ToInt32(splitedaddress.Last()) - 1;

            var result = OutputLastHost.Content =
                splitedaddress[0] + "." + splitedaddress[1] + "." + splitedaddress[2] + "." + newlast;

            return IPAddress.Parse(result.ToString());
        }

        private void DisplayHostsNetz(IPAddress firstHost, IPAddress lastHost)
        {
            var result = (ConvertFromIpAddressToInteger(firstHost) - ConvertFromIpAddressToInteger(lastHost)) * -1 + 1;

            OutputHostsNetz.Content = result;
        }
    }
}