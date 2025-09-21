# DNS4EU

DNS4EU est une application Windows permettant de configurer facilement les serveurs DNS de votre système avec les services DNS européens DNS4EU. L'application fournit une interface graphique conviviale pour sélectionner différents niveaux de protection et gérer automatiquement les paramètres DNS de votre carte réseau active.

![Screen](https://i.imgur.com/ErWo5YL.jpeg)

- [X] Interface utilisateur intuitive
- [X] Configuration DNS automatisée
- [X] Support de multiples niveaux de protection
- [X] Gestion des privilèges administrateur

## Fonctionnalités

### 🛡️ **Niveaux de protection DNS4EU**
- **Protection standard** : Blocage des sites malveillants et de phishing (86.54.11.1 / 86.54.11.201)
- **Protection + Contrôle parental** : Protection standard + blocage du contenu adulte (86.54.11.12 / 86.54.11.212)
- **Protection + Blocage publicités** : Protection standard + blocage des publicités (86.54.11.13 / 86.54.11.213)
- **Protection complète** : Toutes les protections activées (86.54.11.11 / 86.54.11.211)
- **Sans filtrage** : Résolution DNS standard sans filtrage (86.54.11.100 / 86.54.11.200)

### 🔧 **Gestion réseau automatisée**
- **Détection automatique** : Identification de la carte réseau active
- **Configuration WMI** : Modification des paramètres DNS via WMI (Windows Management Instrumentation)
- **Cache DNS** : Vidage automatique du cache DNS après modification
- **Restauration** : Remise en DNS automatique en un clic

### 🔍 **Informations système**
- **Affichage temps réel** : Visualisation des DNS actuellement configurés
- **Détails réseau** : Informations sur la carte réseau active
- **Statut de sécurité** : Indication des paramètres DNS appliqués
- **Gestion d'erreurs** : Diagnostic détaillé en cas de problème

### 💻 **Interface utilisateur**
- **Design moderne** : Interface graphique claire et intuitive
- **Codes couleur** : Boutons colorés pour une navigation facile
- **Descriptions détaillées** : Explication de chaque niveau de protection
- **Actualisation manuelle** : Bouton de rafraîchissement des informations

## Nouveautés de cette version

- **Méthodes de configuration améliorées** : Double approche WMI pour une meilleure compatibilité
- **Gestion d'erreurs robuste** : Messages d'erreur détaillés avec codes WMI traduits
- **Cache DNS automatique** : Vidage automatique pour application immédiate
- **Interface utilisateur repensée** : Design moderne avec codes couleur
- **Stabilité améliorée** : Gestion robuste des interfaces réseau et des erreurs

## Pré-requis

- **.NET Framework 4.8** ou supérieur
- **Windows 7/8/8.1/10/11**
- **Privilèges administrateur** requis pour modifier les paramètres DNS
- **Carte réseau active** avec configuration IP

## Installation

1. Téléchargez l'exécutable depuis les releases
2. Exécutez l'application **en tant qu'administrateur** (clic droit > "Exécuter en tant qu'administrateur")
3. Sélectionnez votre niveau de protection souhaité
4. Cliquez sur "Appliquer les DNS"

## Utilisation

### Configuration des DNS

1. **Lancez l'application** en tant qu'administrateur
2. **Sélectionnez** le niveau de protection désiré parmi les options proposées
3. **Cliquez** sur "Appliquer les DNS" pour configurer votre système
4. **Vérifiez** que les DNS ont été appliqués dans la section "Informations réseau"

### Restauration des paramètres

- **Cliquez** sur "DNS automatique" pour remettre les paramètres par défaut
- **Utilisez** "Actualiser" pour mettre à jour les informations affichées

### Résolution des problèmes

- **Exécution administrateur** : Assurez-vous que l'application est lancée avec les privilèges administrateur
- **Carte réseau** : Vérifiez qu'une carte réseau est active et connectée
- **Erreurs WMI** : Les messages d'erreur détaillés vous indiqueront la cause du problème

## Codes d'état système

- 🟢 **DNS4EU configuré** : Serveurs DNS4EU correctement appliqués
- 🟠 **DNS mixtes** : Configuration partiellement appliquée
- 🔴 **Erreur de configuration** : Échec de la modification des paramètres

## Installation depuis les sources

1. Clonez ce dépôt sur votre machine locale :

   ```bash
   git clone https://github.com/o2Cloud-fr/DNS4EU.git
   ```

2. Ouvrez le projet dans Visual Studio
3. Compilez en mode Release
4. Exécutez l'application générée en tant qu'administrateur

## Configuration requise

- **.NET Framework 4.8** ou supérieur
- **Visual Studio 2019/2022** (pour la compilation)
- **Privilèges administratifs** pour l'exécution
- **Windows Management Instrumentation** (WMI) activé

## Authors

- [@MyAlien](https://www.github.com/MyAlien)
- [@o2Cloud](https://www.github.com/o2Cloud-fr)

## Badges

[![MIT License](https://img.shields.io/badge/License-o2Cloud-yellow.svg)]()
[![.NET](https://img.shields.io/badge/.NET-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![Windows](https://img.shields.io/badge/Windows-7%2B-blue.svg)](https://www.microsoft.com/windows)

## Contributing

Les contributions sont toujours les bienvenues !

Voir `contributing.md` pour savoir comment commencer.

Veuillez respecter le `code de conduite` de ce projet.

## Feedback

Si vous avez des commentaires, n'hésitez pas à nous contacter à github@o2cloud.fr

## 🔗 Links
[![portfolio](https://img.shields.io/badge/my_portfolio-000?style=for-the-badge&logo=ko-fi&logoColor=white)](https://vcard.o2cloud.fr/)
[![linkedin](https://img.shields.io/badge/linkedin-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/remi-simier-2b30142a1/)

## 🛠 Skills
C# • WinForms • WMI • Network Management • DNS Configuration

## License

[Apache-2.0 license](https://github.com/o2Cloud-fr/DNS4EU/blob/main/LICENSE)

![Logo](https://o2cloud.fr/logo/o2Cloud.png)

## Related

Voici quelques projets connexes :

[CheckSec - Vérification Windows Defender](https://github.com/o2Cloud-fr/CheckSec)

## Roadmap

- Support des profils DNS multiples
- Configuration avancée par interface réseau
- Interface en ligne de commande
- Support d'IPv6
- Planification automatique des changements
- Intégration avec d'autres services DNS européens

## Support

Pour le support, envoyez un email à github@o2cloud.fr ou rejoignez notre canal Slack.

## Tech Stack

**Client:** C#, WinForms, .NET Framework 4.8

**System:** WMI (Windows Management Instrumentation), PowerShell

**Network:** DNS Management, IP Configuration

## Used By

Ce projet est utilisé par les entreprises suivantes :

- o2Cloud
- MyAlienTech

## FAQ

### ❓ Pourquoi l'application nécessite-t-elle des privilèges administrateur ?

La modification des paramètres DNS système nécessite des privilèges élevés. L'application utilise WMI pour modifier la configuration réseau, ce qui requiert des droits administrateur.

### ❓ Que faire si les DNS ne changent pas ?

1. Vérifiez que l'application est lancée en tant qu'administrateur
2. Assurez-vous qu'aucun VPN n'interfère
3. Redémarrez votre connexion réseau
4. Consultez les messages d'erreur détaillés

### ❓ Les paramètres DNS4EU sont-ils sécurisés ?

Oui, DNS4EU est un service DNS européen qui respecte le RGPD et offre différents niveaux de protection contre les menaces en ligne.

### ❓ Comment revenir aux DNS par défaut ?

Cliquez simplement sur le bouton "DNS automatique" dans l'application pour restaurer les paramètres par défaut de votre fournisseur d'accès.

### ❓ L'application fonctionne-t-elle avec tous les types de connexion ?

L'application fonctionne avec les connexions Ethernet et Wi-Fi. Elle détecte automatiquement la carte réseau active avec une passerelle configurée.