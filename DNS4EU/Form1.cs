using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private Panel headerPanel;
        private Panel contentPanel;
        private Panel statusPanel;
        private Label statusLabel;
        private ProgressBar statusProgress;

        // Couleurs du thème professionnel
        private readonly Color PrimaryColor = Color.FromArgb(41, 128, 185);      // Bleu professionnel
        private readonly Color SecondaryColor = Color.FromArgb(52, 73, 94);      // Gris bleu foncé
        private readonly Color AccentColor = Color.FromArgb(46, 204, 113);       // Vert success
        private readonly Color WarningColor = Color.FromArgb(230, 126, 34);      // Orange warning
        private readonly Color DangerColor = Color.FromArgb(231, 76, 60);        // Rouge danger
        private readonly Color BackgroundColor = Color.FromArgb(236, 240, 241);  // Gris clair
        private readonly Color CardColor = Color.White;                          // Blanc pour les cartes
        private readonly Color TextColor = Color.FromArgb(44, 62, 80);           // Texte principal
        private readonly Color TextLightColor = Color.FromArgb(127, 140, 141);   // Texte secondaire

        // DNS4EU Configurations
        private readonly Dictionary<string, string[]> dnsConfigs = new Dictionary<string, string[]>
        {
            { "Protection standard", new string[] { "86.54.11.1", "86.54.11.201" } },
            { "Protection + Contrôle parental", new string[] { "86.54.11.12", "86.54.11.212" } },
            { "Protection + Blocage publicités", new string[] { "86.54.11.13", "86.54.11.213" } },
            { "Protection complète (Parental + Pub)", new string[] { "86.54.11.11", "86.54.11.211" } },
            { "Sans filtrage", new string[] { "86.54.11.100", "86.54.11.200" } }
        };

        private readonly Dictionary<string, string> dnsDescriptions = new Dictionary<string, string>
        {
            { "Protection standard", "Bloque les sites malveillants, de phishing et les logiciels malveillants" },
            { "Protection + Contrôle parental", "Protection standard + blocage du contenu adulte et inapproprié" },
            { "Protection + Blocage publicités", "Protection standard + blocage des publicités et trackers" },
            { "Protection complète (Parental + Pub)", "Toutes les protections : malveillants, adultes, publicités" },
            { "Sans filtrage", "Résolution DNS standard sans aucun filtrage" }
        };

        private readonly Dictionary<string, Color> dnsColors = new Dictionary<string, Color>
        {
            { "Protection standard", Color.FromArgb(52, 152, 219) },
            { "Protection + Contrôle parental", Color.FromArgb(155, 89, 182) },
            { "Protection + Blocage publicités", Color.FromArgb(46, 204, 113) },
            { "Protection complète (Parental + Pub)", Color.FromArgb(230, 126, 34) },
            { "Sans filtrage", Color.FromArgb(149, 165, 166) }
        };

        public Form1()
        {
            InitializeComponent();
            InitializeModernUI();
            LoadCurrentDnsSettings();
        }

        private void InitializeModernUI()
        {
            // Configuration de la fenêtre principale
            this.Text = "DNS4EU - Configurateur DNS Professionnel";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new Size(1000, 800);
            this.BackColor = BackgroundColor;
            this.Font = new Font("Segoe UI", 9F);

            // Icône personnalisée
            this.Icon = SystemIcons.Shield;

            CreateHeaderPanel();
            CreateContentPanel();
            CreateStatusPanel();

            LoadCurrentDnsSettings();
        }

        private void CreateHeaderPanel()
        {
            // Panel d'en-tête avec dégradé
            headerPanel = new Panel();
            headerPanel.Size = new Size(this.ClientSize.Width, 90);
            headerPanel.Location = new Point(0, 0);
            headerPanel.Paint += HeaderPanel_Paint;

            // Logo/Icône
            PictureBox logoBox = new PictureBox();
            logoBox.Size = new Size(56, 56);
            logoBox.Location = new Point(25, 17);
            logoBox.Image = SystemIcons.Shield.ToBitmap();
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;
            headerPanel.Controls.Add(logoBox);

            // Titre principal
            Label titleLabel = new Label();
            titleLabel.Text = "DNS4EU";
            titleLabel.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(95, 15);
            titleLabel.BackColor = Color.Transparent;
            headerPanel.Controls.Add(titleLabel);

            // Sous-titre
            Label subtitleLabel = new Label();
            subtitleLabel.Text = "Configurateur DNS Professionnel";
            subtitleLabel.Font = new Font("Segoe UI", 11F);
            subtitleLabel.ForeColor = Color.FromArgb(200, 255, 255, 255);
            subtitleLabel.AutoSize = true;
            subtitleLabel.Location = new Point(95, 50);
            subtitleLabel.BackColor = Color.Transparent;
            headerPanel.Controls.Add(subtitleLabel);

            this.Controls.Add(headerPanel);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            // Dégradé bleu pour l'en-tête
            using (LinearGradientBrush brush = new LinearGradientBrush(
                headerPanel.ClientRectangle,
                PrimaryColor,
                SecondaryColor,
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
            }
        }

        private void CreateContentPanel()
        {
            contentPanel = new Panel();
            contentPanel.Size = new Size(this.ClientSize.Width - 50, 580);
            contentPanel.Location = new Point(25, 110);
            contentPanel.BackColor = Color.Transparent;

            // Description
            Label descLabel = CreateStyledLabel("Choisissez votre niveau de protection DNS :",
                new Font("Segoe UI", 12F), TextColor);
            descLabel.Location = new Point(0, 0);
            contentPanel.Controls.Add(descLabel);

            // Panel des options DNS avec style moderne
            Panel dnsOptionsPanel = CreateStyledPanel("Options de protection", new Size(925, 380), new Point(0, 35));

            // Création des cartes d'options DNS avec GroupBox pour regrouper les RadioButtons
            int yPos = 50;
            bool isFirst = true;
            foreach (var config in dnsConfigs)
            {
                Panel optionCard = CreateDnsOptionCard(config.Key, config.Value, yPos, isFirst);
                dnsOptionsPanel.Controls.Add(optionCard);
                yPos += 65;
                isFirst = false;
            }

            contentPanel.Controls.Add(dnsOptionsPanel);

            // Panel d'informations réseau
            Panel networkInfoPanel = CreateNetworkInfoPanel();
            networkInfoPanel.Location = new Point(0, 430);
            contentPanel.Controls.Add(networkInfoPanel);

            this.Controls.Add(contentPanel);
        }

        private Panel CreateStyledPanel(string title, Size size, Point location)
        {
            Panel panel = new Panel();
            panel.Size = size;
            panel.Location = location;
            panel.BackColor = CardColor;
            panel.Paint += (s, e) => {
                // Bordure subtile
                using (Pen borderPen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
                }

                // Ombre légère
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, 2, 2, panel.Width - 2, panel.Height - 2);
                }
            };

            // Titre du panel
            Label titleLabel = CreateStyledLabel(title, new Font("Segoe UI", 12F, FontStyle.Bold), SecondaryColor);
            titleLabel.Location = new Point(15, 10);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Panel CreateDnsOptionCard(string configName, string[] dnsServers, int yPos, bool isSelected)
        {
            Panel card = new Panel();
            card.Size = new Size(895, 55);
            card.Location = new Point(15, yPos);
            card.BackColor = CardColor;
            card.Cursor = Cursors.Hand;

            // RadioButton personnalisé - IMPORTANT: ajouter au même parent pour grouping
            RadioButton rb = new RadioButton();
            rb.Name = "rb" + configName.Replace(" ", "").Replace("+", "").Replace("(", "").Replace(")", "");
            rb.Size = new Size(24, 24);
            rb.Location = new Point(15, 16);
            rb.Tag = dnsServers;
            rb.Checked = isSelected;
            rb.FlatStyle = FlatStyle.Flat;
            rb.CheckedChanged += RadioButton_CheckedChanged;

            // Indicateur de couleur
            Panel colorIndicator = new Panel();
            colorIndicator.Size = new Size(5, 45);
            colorIndicator.Location = new Point(0, 5);
            colorIndicator.BackColor = dnsColors[configName];
            card.Controls.Add(colorIndicator);

            // Nom de la configuration
            Label nameLabel = CreateStyledLabel(configName, new Font("Segoe UI", 12F, FontStyle.Bold), TextColor);
            nameLabel.Location = new Point(50, 8);
            nameLabel.AutoSize = true;
            card.Controls.Add(nameLabel);

            // Description
            Label descLabel = CreateStyledLabel(dnsDescriptions[configName],
                new Font("Segoe UI", 10F), TextLightColor);
            descLabel.Location = new Point(50, 30);
            descLabel.AutoSize = true;
            descLabel.MaximumSize = new Size(600, 0);
            card.Controls.Add(descLabel);

            // Adresses DNS - positionnées plus à droite avec plus d'espace
            Label dnsLabel = CreateStyledLabel($"Primaire : {dnsServers[0]}",
                new Font("Consolas", 10F, FontStyle.Bold), Color.FromArgb(108, 117, 125));
            dnsLabel.Location = new Point(680, 12);
            dnsLabel.AutoSize = true;
            card.Controls.Add(dnsLabel);

            Label dnsSecondaryLabel = CreateStyledLabel($"Secondaire : {dnsServers[1]}",
                new Font("Consolas", 10F, FontStyle.Bold), Color.FromArgb(108, 117, 125));
            dnsSecondaryLabel.Location = new Point(680, 32);
            dnsSecondaryLabel.AutoSize = true;
            card.Controls.Add(dnsSecondaryLabel);

            card.Controls.Add(rb);

            // Effet de hover
            card.MouseEnter += (s, e) => {
                if (!rb.Checked)
                    card.BackColor = Color.FromArgb(248, 249, 250);
            };
            card.MouseLeave += (s, e) => {
                if (!rb.Checked)
                    card.BackColor = CardColor;
            };

            // Clic sur la carte sélectionne le radio button
            card.Click += (s, e) => {
                // Décocher tous les autres RadioButtons
                UncheckAllRadioButtons();
                rb.Checked = true;
            };

            return card;
        }

        private void UncheckAllRadioButtons()
        {
            foreach (Control control in contentPanel.Controls)
            {
                if (control is Panel && control.Controls.Count > 0)
                {
                    foreach (Control panelControl in control.Controls)
                    {
                        if (panelControl is Panel card)
                        {
                            foreach (Control cardControl in card.Controls)
                            {
                                if (cardControl is RadioButton rb)
                                {
                                    rb.Checked = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                // Décocher tous les autres RadioButtons quand un est sélectionné
                UncheckAllRadioButtonsExcept(rb);

                // Mettre à jour l'apparence des cartes
                UpdateCardSelection();
            }
        }

        private void UncheckAllRadioButtonsExcept(RadioButton selectedRadio)
        {
            foreach (Control control in contentPanel.Controls)
            {
                if (control is Panel && control.Controls.Count > 0)
                {
                    foreach (Control panelControl in control.Controls)
                    {
                        if (panelControl is Panel card)
                        {
                            foreach (Control cardControl in card.Controls)
                            {
                                if (cardControl is RadioButton rb && rb != selectedRadio)
                                {
                                    rb.Checked = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateCardSelection()
        {
            foreach (Control control in contentPanel.Controls)
            {
                if (control is Panel && control.Controls.Count > 0)
                {
                    foreach (Control panelControl in control.Controls)
                    {
                        if (panelControl is Panel card && card.Controls.OfType<RadioButton>().Any())
                        {
                            RadioButton rb = card.Controls.OfType<RadioButton>().First();
                            if (rb.Checked)
                            {
                                card.BackColor = Color.FromArgb(240, 248, 255);
                            }
                            else
                            {
                                card.BackColor = CardColor;
                            }
                        }
                    }
                }
            }
        }

        private Panel CreateNetworkInfoPanel()
        {
            Panel networkPanel = CreateStyledPanel("Informations réseau", new Size(925, 120), new Point(0, 0));

            // Icône réseau
            PictureBox networkIcon = new PictureBox();
            networkIcon.Size = new Size(32, 32);
            networkIcon.Location = new Point(20, 50);
            networkIcon.Image = SystemIcons.Information.ToBitmap();
            networkIcon.SizeMode = PictureBoxSizeMode.Zoom;
            networkPanel.Controls.Add(networkIcon);

            Label networkLabel = CreateStyledLabel("Chargement...", new Font("Segoe UI", 11F), TextColor);
            networkLabel.Name = "networkLabel";
            networkLabel.Location = new Point(60, 45);
            networkLabel.AutoSize = true;
            networkLabel.MaximumSize = new Size(830, 0);
            networkPanel.Controls.Add(networkLabel);

            Label currentDnsLabel = CreateStyledLabel("DNS : Chargement...", new Font("Consolas", 10F), TextLightColor);
            currentDnsLabel.Name = "currentDnsLabel";
            currentDnsLabel.Location = new Point(60, 70);
            currentDnsLabel.AutoSize = true;
            currentDnsLabel.MaximumSize = new Size(830, 0);
            networkPanel.Controls.Add(currentDnsLabel);

            // Label d'état de connexion
            Label connectionLabel = CreateStyledLabel("État : Vérification...", new Font("Segoe UI", 10F), TextLightColor);
            connectionLabel.Name = "connectionLabel";
            connectionLabel.Location = new Point(60, 95);
            connectionLabel.AutoSize = true;
            connectionLabel.MaximumSize = new Size(830, 0);
            networkPanel.Controls.Add(connectionLabel);

            return networkPanel;
        }

        private void CreateStatusPanel()
        {
            statusPanel = new Panel();
            statusPanel.Size = new Size(this.ClientSize.Width, 90);
            statusPanel.Location = new Point(0, this.ClientSize.Height - 90);
            statusPanel.BackColor = Color.FromArgb(248, 249, 250);
            statusPanel.Paint += (s, e) => {
                using (Pen borderPen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawLine(borderPen, 0, 0, statusPanel.Width, 0);
                }
            };

            // Boutons d'action avec style moderne
            Button applyButton = CreateStyledButton("Appliquer DNS", AccentColor, Color.White, new Size(140, 45));
            applyButton.Location = new Point(25, 22);
            applyButton.Click += ApplyButton_Click;
            statusPanel.Controls.Add(applyButton);

            Button resetButton = CreateStyledButton("DNS Automatique", WarningColor, Color.White, new Size(150, 45));
            resetButton.Location = new Point(175, 22);
            resetButton.Click += ResetButton_Click;
            statusPanel.Controls.Add(resetButton);

            Button refreshButton = CreateStyledButton("Actualiser", PrimaryColor, Color.White, new Size(120, 45));
            refreshButton.Location = new Point(335, 22);
            refreshButton.Click += RefreshButton_Click;
            statusPanel.Controls.Add(refreshButton);

            Button exitButton = CreateStyledButton("Quitter", Color.FromArgb(108, 117, 125), Color.White, new Size(100, 45));
            exitButton.Location = new Point(870, 22);
            exitButton.Click += (s, e) => this.Close();
            statusPanel.Controls.Add(exitButton);

            // Label de statut avec plus d'espace et sur deux lignes si nécessaire
            statusLabel = CreateStyledLabel("Prêt", new Font("Segoe UI", 11F), TextLightColor);
            statusLabel.Location = new Point(470, 25);
            statusLabel.Name = "statusLabel";
            statusLabel.AutoSize = true;
            statusLabel.MaximumSize = new Size(390, 50);
            statusPanel.Controls.Add(statusLabel);

            // Label d'information supplémentaire
            Label infoLabel = CreateStyledLabel("Application lancée avec privilèges administrateur",
                new Font("Segoe UI", 9F), Color.FromArgb(127, 140, 141));
            infoLabel.Name = "infoLabel";
            infoLabel.Location = new Point(470, 50);
            infoLabel.AutoSize = true;
            infoLabel.MaximumSize = new Size(390, 0);
            statusPanel.Controls.Add(infoLabel);

            this.Controls.Add(statusPanel);
        }

        private Button CreateStyledButton(string text, Color backColor, Color foreColor, Size size)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = size;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backColor, 0.1f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backColor, 0.1f);
            button.Cursor = Cursors.Hand;

            // Coins arrondis simulés avec Paint
            button.Paint += (s, e) => {
                Button btn = s as Button;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddRoundedRectangle(btn.ClientRectangle, 4);
                    btn.Region = new Region(path);
                }
            };

            return button;
        }

        private Label CreateStyledLabel(string text, Font font, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = font;
            label.ForeColor = color;
            label.AutoSize = true;
            label.BackColor = Color.Transparent;
            return label;
        }

        private void ShowStatusMessage(string message, Color color, bool showProgress = false)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = message;
                statusLabel.ForeColor = color;
            }

            Application.DoEvents();
        }

        // [Le reste des méthodes existantes restent inchangées]
        private void LoadCurrentDnsSettings()
        {
            ShowStatusMessage("Chargement des paramètres réseau...", PrimaryColor);

            try
            {
                activeNetworkInterface = GetActiveNetworkInterface();
                if (activeNetworkInterface != null)
                {
                    GetCurrentDnsServers();
                    UpdateNetworkInfo();
                    ShowStatusMessage("Prêt", TextLightColor);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des paramètres réseau : {ex.Message}",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowStatusMessage("Erreur lors du chargement", DangerColor);
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
            Label connectionLabel = this.Controls.Find("connectionLabel", true).FirstOrDefault() as Label;

            if (activeNetworkInterface != null)
            {
                if (networkLabel != null)
                    networkLabel.Text = $"Interface : {activeNetworkInterface.Name} ({activeNetworkInterface.NetworkInterfaceType})";

                if (currentDnsLabel != null)
                    currentDnsLabel.Text = $"DNS actuels : {currentPrimaryDns} • {currentSecondaryDns}";

                if (connectionLabel != null)
                {
                    string connectionStatus = activeNetworkInterface.OperationalStatus == OperationalStatus.Up ? "✅ Connectée" : "❌ Déconnectée";
                    connectionLabel.Text = $"État : {connectionStatus} - Vitesse : {(activeNetworkInterface.Speed / 1000000)} Mbps";
                }
            }
            else
            {
                if (networkLabel != null)
                    networkLabel.Text = "❌ Aucune interface réseau active détectée";

                if (currentDnsLabel != null)
                    currentDnsLabel.Text = "DNS : Non disponible";

                if (connectionLabel != null)
                    connectionLabel.Text = "État : Interface non disponible";
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

            ShowStatusMessage("Application des paramètres DNS...", PrimaryColor);

            // Trouver le RadioButton sélectionné
            RadioButton selectedRadio = null;
            foreach (Control control in contentPanel.Controls)
            {
                if (control is Panel)
                {
                    foreach (Control panelControl in control.Controls)
                    {
                        if (panelControl is Panel card)
                        {
                            foreach (Control cardControl in card.Controls)
                            {
                                if (cardControl is RadioButton rb && rb.Checked)
                                {
                                    selectedRadio = rb;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (selectedRadio?.Tag is string[] dnsServers && dnsServers.Length >= 2)
            {
                try
                {
                    SetDnsServersImproved(activeNetworkInterface, dnsServers[0], dnsServers[1]);
                    MessageBox.Show($"✅ DNS modifiés avec succès !\n\n📡 Primaire : {dnsServers[0]}\n📡 Secondaire : {dnsServers[1]}\n\n🔄 Les changements sont effectifs immédiatement.",
                                   "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ShowStatusMessage("DNS appliqués avec succès", AccentColor);

                    // Actualiser les informations
                    System.Threading.Thread.Sleep(2000);
                    LoadCurrentDnsSettings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Erreur lors de l'application des DNS :\n\n{ex.Message}\n\n🔐 Assurez-vous d'exécuter l'application en tant qu'administrateur.",
                                   "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowStatusMessage("Erreur lors de l'application", DangerColor);
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

            ShowStatusMessage("Remise en mode automatique...", WarningColor);

            try
            {
                // Utiliser la méthode Windows netsh comme alternative plus fiable
                bool success = ResetDnsUsingNetsh(activeNetworkInterface);

                if (!success)
                {
                    // Fallback vers la méthode WMI améliorée
                    ResetDnsToAutomaticImproved(activeNetworkInterface);
                }

                MessageBox.Show("✅ DNS remis en automatique avec succès !\n\n🔄 Votre système utilisera maintenant les DNS fournis par votre FAI ou DHCP.",
                               "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ShowStatusMessage("DNS remis en automatique", AccentColor);

                // Actualiser les informations
                System.Threading.Thread.Sleep(2000);
                LoadCurrentDnsSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de la remise en automatique :\n\n{ex.Message}\n\n🔐 Assurez-vous d'exécuter l'application en tant qu'administrateur.",
                               "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowStatusMessage("Erreur lors de la remise en automatique", DangerColor);
            }
        }

        // Méthode alternative utilisant netsh (plus fiable)
        private bool ResetDnsUsingNetsh(NetworkInterface networkInterface)
        {
            try
            {
                string interfaceName = networkInterface.Name;

                // Commande netsh pour remettre les DNS en automatique
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip set dns \"{interfaceName}\" dhcp",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = System.Diagnostics.Process.Start(processInfo))
                {
                    process.WaitForExit(10000); // Timeout de 10 secondes

                    if (process.ExitCode == 0)
                    {
                        FlushDnsCache();
                        return true;
                    }
                    else
                    {
                        string error = process.StandardError.ReadToEnd();
                        throw new Exception($"Netsh a échoué : {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Si netsh échoue, on laissera la méthode WMI essayer
                System.Diagnostics.Debug.WriteLine($"Netsh failed: {ex.Message}");
                return false;
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadCurrentDnsSettings();
            MessageBox.Show("🔄 Informations actualisées avec succès !", "Information",
                           MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // [Toutes les autres méthodes de l'application originale restent inchangées]
        private void SetDnsServersImproved(NetworkInterface networkInterface, string primaryDns, string secondaryDns)
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

                FlushDnsCache();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la configuration DNS : {ex.Message}");
            }
        }

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

                FlushDnsCache();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la remise en automatique : {ex.Message}");
            }
        }

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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Mise à jour du label d'information
            Label infoLabel = this.Controls.Find("infoLabel", true).FirstOrDefault() as Label;

            // Vérification si l'application est exécutée en tant qu'administrateur
            if (!IsRunAsAdministrator())
            {
                ShowDialog("⚠️ PRIVILÈGES ADMINISTRATEUR REQUIS",
                          "Pour modifier les paramètres DNS, cette application doit être exécutée en tant qu'administrateur.\n\n" +
                          "🔐 Solution :\n" +
                          "• Fermez l'application\n" +
                          "• Faites un clic droit sur l'exécutable\n" +
                          "• Sélectionnez 'Exécuter en tant qu'administrateur'\n\n" +
                          "⚡ L'application continuera de fonctionner mais les modifications DNS échoueront.",
                          MessageBoxIcon.Warning);
                ShowStatusMessage("⚠️ Privilèges administrateur requis", WarningColor);

                if (infoLabel != null)
                    infoLabel.Text = "⚠️ Privilèges administrateur requis pour modifier les DNS";
            }
            else
            {
                ShowStatusMessage("✅ Application lancée avec privilèges administrateur", AccentColor);

                if (infoLabel != null)
                    infoLabel.Text = "✅ Application lancée avec privilèges administrateur";
            }
        }

        private void ShowDialog(string title, string message, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        private bool IsRunAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Ajuster la position du panel de statut
            if (statusPanel != null)
            {
                statusPanel.Location = new Point(0, this.ClientSize.Height - 90);
                statusPanel.Size = new Size(this.ClientSize.Width, 90);
            }

            // Ajuster la largeur de l'en-tête
            if (headerPanel != null)
            {
                headerPanel.Size = new Size(this.ClientSize.Width, 90);
            }

            // Ajuster la largeur du contenu
            if (contentPanel != null)
            {
                contentPanel.Size = new Size(this.ClientSize.Width - 50, 580);
            }
        }
    }

    // Extension pour les coins arrondis
    public static class GraphicsExtensions
    {
        public static void AddRoundedRectangle(this GraphicsPath path, Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

            // Coin supérieur gauche
            path.AddArc(arc, 180, 90);

            // Coin supérieur droit
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Coin inférieur droit
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Coin inférieur gauche
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
        }
    }
}