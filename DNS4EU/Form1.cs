using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Net.NetworkInformation;
using System.Net;
using System.Security.Principal;

namespace DNS4EU
{
    public partial class Form1 : Form
    {
        private NetworkInterface activeNetworkInterface;
        private string currentPrimaryDns = "";
        private string currentSecondaryDns = "";

        // DNS4EU Configurations
        private readonly Dictionary<string, string[]> dnsConfigs = new Dictionary<string, string[]>
        {
            { "Protection standard", new string[] { "86.54.11.1", "86.54.11.201" } },
            { "Protection + Contrôle parental", new string[] { "86.54.11.12", "86.54.11.212" } },
            { "Protection + Blocage publicités", new string[] { "86.54.11.13", "86.54.11.213" } },
            { "Protection complète (Parental + Pub)", new string[] { "86.54.11.11", "86.54.11.211" } },
            { "Sans filtrage", new string[] { "86.54.11.100", "86.54.11.200" } }
        };

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            LoadCurrentDnsSettings();
        }

        private void InitializeUI()
        {
            // Configuration de la fenêtre
            this.Text = "DNS4EU - Configurateur DNS";
            this.Size = new Size(550, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = SystemIcons.Shield;

            // Titre principal
            Label titleLabel = new Label();
            titleLabel.Text = "DNS4EU - Configurateur DNS";
            titleLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.DarkBlue;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(20, 20);
            this.Controls.Add(titleLabel);

            // Description
            Label descLabel = new Label();
            descLabel.Text = "Choisissez votre niveau de protection DNS :";
            descLabel.Font = new Font("Arial", 10);
            descLabel.AutoSize = true;
            descLabel.Location = new Point(20, 60);
            this.Controls.Add(descLabel);

            // GroupBox pour les options DNS
            GroupBox dnsGroupBox = new GroupBox();
            dnsGroupBox.Text = "Options de protection";
            dnsGroupBox.Size = new Size(500, 200);
            dnsGroupBox.Location = new Point(20, 85);
            dnsGroupBox.Font = new Font("Arial", 9, FontStyle.Bold);

            // RadioButtons pour chaque configuration DNS
            int yPos = 25;
            bool isFirst = true;
            foreach (var config in dnsConfigs)
            {
                RadioButton rb = new RadioButton();
                rb.Name = "rb" + config.Key.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "");
                rb.Text = config.Key;
                rb.Font = new Font("Arial", 9);
                rb.AutoSize = true;
                rb.Location = new Point(20, yPos);
                rb.Tag = config.Value;

                if (isFirst)
                {
                    rb.Checked = true;
                    isFirst = false;
                }

                // Ajout de descriptions pour chaque option
                Label desc = new Label();
                desc.Font = new Font("Arial", 8);
                desc.ForeColor = Color.Gray;
                desc.AutoSize = true;
                desc.Location = new Point(40, yPos + 20);

                switch (config.Key)
                {
                    case "Protection standard":
                        desc.Text = "Bloque les sites malveillants et de phishing";
                        break;
                    case "Protection + Contrôle parental":
                        desc.Text = "Protection standard + blocage contenu adulte";
                        break;
                    case "Protection + Blocage publicités":
                        desc.Text = "Protection standard + blocage des publicités";
                        break;
                    case "Protection complète (Parental + Pub)":
                        desc.Text = "Toutes les protections activées";
                        break;
                    case "Sans filtrage":
                        desc.Text = "Aucun filtrage, résolution DNS standard";
                        break;
                }

                dnsGroupBox.Controls.Add(rb);
                dnsGroupBox.Controls.Add(desc);
                yPos += 35;
            }

            this.Controls.Add(dnsGroupBox);

            // Informations sur la carte réseau active
            GroupBox networkInfoBox = new GroupBox();
            networkInfoBox.Text = "Informations réseau";
            networkInfoBox.Size = new Size(500, 80);
            networkInfoBox.Location = new Point(20, 295);
            networkInfoBox.Font = new Font("Arial", 9, FontStyle.Bold);

            Label networkLabel = new Label();
            networkLabel.Name = "networkLabel";
            networkLabel.Font = new Font("Arial", 8);
            networkLabel.AutoSize = true;
            networkLabel.Location = new Point(10, 25);
            networkInfoBox.Controls.Add(networkLabel);

            Label currentDnsLabel = new Label();
            currentDnsLabel.Name = "currentDnsLabel";
            currentDnsLabel.Font = new Font("Arial", 8);
            currentDnsLabel.AutoSize = true;
            currentDnsLabel.Location = new Point(10, 45);
            networkInfoBox.Controls.Add(currentDnsLabel);

            this.Controls.Add(networkInfoBox);

            // Boutons
            Button applyButton = new Button();
            applyButton.Text = "Appliquer les DNS";
            applyButton.Size = new Size(120, 35);
            applyButton.Location = new Point(20, 390);
            applyButton.BackColor = Color.Green;
            applyButton.ForeColor = Color.White;
            applyButton.Font = new Font("Arial", 9, FontStyle.Bold);
            applyButton.Click += ApplyButton_Click;
            this.Controls.Add(applyButton);

            Button resetButton = new Button();
            resetButton.Text = "DNS automatique";
            resetButton.Size = new Size(120, 35);
            resetButton.Location = new Point(150, 390);
            resetButton.BackColor = Color.Orange;
            resetButton.ForeColor = Color.White;
            resetButton.Font = new Font("Arial", 9, FontStyle.Bold);
            resetButton.Click += ResetButton_Click;
            this.Controls.Add(resetButton);

            Button refreshButton = new Button();
            refreshButton.Text = "Actualiser";
            refreshButton.Size = new Size(100, 35);
            refreshButton.Location = new Point(280, 390);
            refreshButton.BackColor = Color.Blue;
            refreshButton.ForeColor = Color.White;
            refreshButton.Font = new Font("Arial", 9, FontStyle.Bold);
            refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(refreshButton);

            Button exitButton = new Button();
            exitButton.Text = "Quitter";
            exitButton.Size = new Size(100, 35);
            exitButton.Location = new Point(420, 390);
            exitButton.BackColor = Color.Gray;
            exitButton.ForeColor = Color.White;
            exitButton.Font = new Font("Arial", 9, FontStyle.Bold);
            exitButton.Click += (s, e) => this.Close();
            this.Controls.Add(exitButton);

            UpdateNetworkInfo();
        }

        private void LoadCurrentDnsSettings()
        {
            try
            {
                activeNetworkInterface = GetActiveNetworkInterface();
                if (activeNetworkInterface != null)
                {
                    GetCurrentDnsServers();
                    UpdateNetworkInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des paramètres réseau : {ex.Message}",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private NetworkInterface GetActiveNetworkInterface()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    if (ipProps.GatewayAddresses.Count > 0)
                    {
                        return ni;
                    }
                }
            }
            return null;
        }

        private void GetCurrentDnsServers()
        {
            if (activeNetworkInterface == null) return;

            try
            {
                IPInterfaceProperties ipProps = activeNetworkInterface.GetIPProperties();
                var dnsServers = ipProps.DnsAddresses;

                if (dnsServers.Count > 0)
                {
                    currentPrimaryDns = dnsServers[0].ToString();
                    if (dnsServers.Count > 1)
                        currentSecondaryDns = dnsServers[1].ToString();
                    else
                        currentSecondaryDns = "Aucun";
                }
                else
                {
                    currentPrimaryDns = "Automatique";
                    currentSecondaryDns = "Automatique";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la lecture des DNS : {ex.Message}",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateNetworkInfo()
        {
            Label networkLabel = this.Controls.Find("networkLabel", true).FirstOrDefault() as Label;
            Label currentDnsLabel = this.Controls.Find("currentDnsLabel", true).FirstOrDefault() as Label;

            if (activeNetworkInterface != null)
            {
                if (networkLabel != null)
                    networkLabel.Text = $"Carte active : {activeNetworkInterface.Name}";

                if (currentDnsLabel != null)
                    currentDnsLabel.Text = $"DNS actuels : {currentPrimaryDns} / {currentSecondaryDns}";
            }
            else
            {
                if (networkLabel != null)
                    networkLabel.Text = "Aucune carte réseau active détectée";

                if (currentDnsLabel != null)
                    currentDnsLabel.Text = "DNS : Non disponible";
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (activeNetworkInterface == null)
            {
                MessageBox.Show("Aucune carte réseau active détectée.", "Erreur",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Trouver le RadioButton sélectionné
            RadioButton selectedRadio = null;
            foreach (Control control in this.Controls)
            {
                if (control is GroupBox groupBox)
                {
                    foreach (Control gbControl in groupBox.Controls)
                    {
                        if (gbControl is RadioButton rb && rb.Checked)
                        {
                            selectedRadio = rb;
                            break;
                        }
                    }
                }
            }

            if (selectedRadio?.Tag is string[] dnsServers && dnsServers.Length >= 2)
            {
                try
                {
                    // Nouvelle méthode améliorée
                    SetDnsServersImproved(activeNetworkInterface, dnsServers[0], dnsServers[1]);
                    MessageBox.Show($"DNS modifiés avec succès !\n\nPrimaire : {dnsServers[0]}\nSecondaire : {dnsServers[1]}",
                                   "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Actualiser les informations
                    System.Threading.Thread.Sleep(2000); // Augmenté à 2 secondes
                    LoadCurrentDnsSettings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'application des DNS : {ex.Message}\n\nAssurez-vous d'exécuter l'application en tant qu'administrateur.",
                                   "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            if (activeNetworkInterface == null)
            {
                MessageBox.Show("Aucune carte réseau active détectée.", "Erreur",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ResetDnsToAutomaticImproved(activeNetworkInterface);
                MessageBox.Show("DNS remis en automatique avec succès !", "Succès",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Actualiser les informations
                System.Threading.Thread.Sleep(2000);
                LoadCurrentDnsSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la remise en automatique : {ex.Message}\n\nAssurez-vous d'exécuter l'application en tant qu'administrateur.",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadCurrentDnsSettings();
            MessageBox.Show("Informations actualisées !", "Information",
                           MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Méthode améliorée pour définir les serveurs DNS
        private void SetDnsServersImproved(NetworkInterface networkInterface, string primaryDns, string secondaryDns)
        {
            try
            {
                // Méthode 1: Essayer avec l'ID de l'interface
                string query = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE InterfaceIndex={networkInterface.GetIPProperties().GetIPv4Properties().Index}";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    ManagementObjectCollection results = searcher.Get();

                    if (results.Count == 0)
                    {
                        // Méthode 2: Essayer avec le nom de l'adaptateur
                        query = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Description LIKE '%{networkInterface.Description.Replace("'", "''")}%'";
                        searcher.Query = new ObjectQuery(query);
                        results = searcher.Get();
                    }

                    bool found = false;
                    foreach (ManagementObject mo in results)
                    {
                        if ((bool)mo["IPEnabled"])
                        {
                            ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                            newDNS["DNSServerSearchOrder"] = new string[] { primaryDns, secondaryDns };
                            ManagementBaseObject result = mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);

                            uint returnValue = (uint)result.Properties["returnValue"].Value;
                            if (returnValue != 0)
                            {
                                throw new Exception($"Échec WMI. Code d'erreur : {returnValue}. Description : {GetWMIErrorDescription(returnValue)}");
                            }
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        throw new Exception("Aucune carte réseau compatible trouvée ou carte non activée pour IP.");
                    }
                }

                // Vider le cache DNS
                FlushDnsCache();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la configuration DNS : {ex.Message}");
            }
        }

        // Méthode améliorée pour remettre les DNS en automatique
        private void ResetDnsToAutomaticImproved(NetworkInterface networkInterface)
        {
            try
            {
                string query = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE InterfaceIndex={networkInterface.GetIPProperties().GetIPv4Properties().Index}";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    ManagementObjectCollection results = searcher.Get();

                    if (results.Count == 0)
                    {
                        query = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Description LIKE '%{networkInterface.Description.Replace("'", "''")}%'";
                        searcher.Query = new ObjectQuery(query);
                        results = searcher.Get();
                    }

                    bool found = false;
                    foreach (ManagementObject mo in results)
                    {
                        if ((bool)mo["IPEnabled"])
                        {
                            // Passer null pour remettre en automatique
                            ManagementBaseObject result = mo.InvokeMethod("SetDNSServerSearchOrder", null, null);

                            uint returnValue = (uint)result.Properties["returnValue"].Value;
                            if (returnValue != 0)
                            {
                                throw new Exception($"Échec WMI. Code d'erreur : {returnValue}. Description : {GetWMIErrorDescription(returnValue)}");
                            }
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        throw new Exception("Aucune carte réseau compatible trouvée.");
                    }
                }

                // Vider le cache DNS
                FlushDnsCache();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la remise en automatique : {ex.Message}");
            }
        }

        // Vider le cache DNS
        private void FlushDnsCache()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }).WaitForExit();
            }
            catch
            {
                // Ignorer les erreurs de flush DNS
            }
        }

        // Descriptions des codes d'erreur WMI
        private string GetWMIErrorDescription(uint errorCode)
        {
            switch (errorCode)
            {
                case 0: return "Succès";
                case 1: return "Succès - Redémarrage requis";
                case 64: return "Méthode non supportée";
                case 65: return "Accès refusé";
                case 66: return "Instance non trouvée";
                case 67: return "Classe non trouvée";
                case 68: return "Paramètre invalide";
                case 69: return "Propriété en lecture seule";
                case 70: return "Fournisseur non capable";
                case 71: return "Classe possède des instances";
                case 72: return "Paramètre invalide";
                case 73: return "Instance invalide";
                case 74: return "Nom invalide";
                case 75: return "Classe invalide";
                case 76: return "Fournisseur invalide";
                case 77: return "Espace de noms invalide";
                case 78: return "Opération invalide";
                case 79: return "Paramètre dupliqué";
                case 80: return "Index invalide";
                case 81: return "Objet inconnu";
                case 82: return "Limite invalide";
                case 83: return "Contexte invalide";
                case 84: return "Déjà existe";
                case 85: return "Classe non dérivée";
                case 86: return "Superclasse manquante";
                case 87: return "Classe possède des enfants";
                case 88: return "Classe possède des instances";
                case 89: return "Requête invalide";
                case 90: return "Langue de requête non supportée";
                case 91: return "Requête invalide";
                case 92: return "Requête trop complexe";
                case 93: return "Propriété invalide";
                case 94: return "Fournisseur suspendu";
                default: return $"Erreur inconnue ({errorCode})";
            }
        }

        // Anciennes méthodes conservées comme fallback
        private void SetDnsServers(string adapterName, string primaryDns, string secondaryDns)
        {
            string query = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Description='{adapterName.Replace("'", "''")}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                        newDNS["DNSServerSearchOrder"] = new string[] { primaryDns, secondaryDns };
                        ManagementBaseObject result = mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);

                        uint returnValue = (uint)result.Properties["returnValue"].Value;
                        if (returnValue != 0)
                        {
                            throw new Exception($"Échec de la modification DNS. Code d'erreur : {returnValue}");
                        }
                        break;
                    }
                }
            }
        }

        private void ResetDnsToAutomatic(string adapterName)
        {
            string query = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Description='{adapterName.Replace("'", "''")}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        ManagementBaseObject result = mo.InvokeMethod("SetDNSServerSearchOrder", null, null);

                        uint returnValue = (uint)result.Properties["returnValue"].Value;
                        if (returnValue != 0)
                        {
                            throw new Exception($"Échec de la remise en automatique. Code d'erreur : {returnValue}");
                        }
                        break;
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Vérification si l'application est exécutée en tant qu'administrateur
            if (!IsRunAsAdministrator())
            {
                MessageBox.Show("⚠️ ATTENTION ⚠️\n\nPour modifier les paramètres DNS, cette application doit être exécutée en tant qu'administrateur.\n\nFaites un clic droit sur l'application et sélectionnez 'Exécuter en tant qu'administrateur'.",
                               "Privilèges requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsRunAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
    }
}