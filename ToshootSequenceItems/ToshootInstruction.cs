using Cyrilastro.NINA.Toshoot.Properties;
using Newtonsoft.Json;
using NINA.Astrometry.Interfaces;
using NINA.Core.Model;
using NINA.Core.Utility.Notification;
using NINA.Sequencer.SequenceItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NINA.Core.Enum;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container.ExecutionStrategy;
using NINA.Sequencer.Trigger;
using NINA.Core.Utility;
using NINA.Astrometry;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.Equipment.Exceptions;
using NINA.Core.Locale;
using NINA.Equipment.Interfaces;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.Sequencer.Interfaces;
using NINA.Core.Model.Equipment;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem.Imaging;
using NINA.Sequencer.Utility;
using System.IO;
using System.Windows.Controls;
using System.Xml.Linq;
using NINA.Equipment.Equipment.MyCamera;
using NINA.WPF.Base.SkySurvey;
using System.Security.Cryptography;
using NINA.Sequencer.Interfaces.Mediator;
using NINA.Sequencer.Container;
using System.ComponentModel;
using NINA.Profile;
using NINA.Sequencer;
using NINA.WPF.Base.Mediator;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Namotion.Reflection;
using Google.Protobuf;
using NINA.Sequencer.DragDrop;
using System.Windows.Threading;


namespace Cyrilastro.NINA.Toshoot.ToshootTestCategory {
    /// <summary>
    /// This Class shows the basic principle on how to add a new Sequence Instruction to the N.I.N.A. sequencer via the plugin interface
    /// For ease of use this class inherits the abstract SequenceItem which already handles most of the running logic, like logging, exception handling etc.
    /// A complete custom implementation by just implementing ISequenceItem is possible too
    /// The following MetaData can be set to drive the initial values
    /// --> Name - The name that will be displayed for the item
    /// --> Description - a brief summary of what the item is doing. It will be displayed as a tooltip on mouseover in the application
    /// --> Icon - a string to the key value of a Geometry inside N.I.N.A.'s geometry resources
    ///
    /// If the item has some preconditions that should be validated, it shall also extend the IValidatable interface and add the validation logic accordingly.
    /// </summary>
    [ExportMetadata("Name", "ToShoot Instruction")]
    [ExportMetadata("Description", "This item will just show a notification and is just there to show how the plugin system works")]
    [ExportMetadata("Icon", "Plugin_Test_SVG")]
    [ExportMetadata("Category", "Toshoot")]
    [Export(typeof(ISequenceItem))]
    [Export(typeof(ISequenceContainer))]
    [JsonObject(MemberSerialization.OptIn)]
    public class ToshootInstruction : SequenceItem, ISequenceItem{
        private  IFramingAssistantVM framingAssistantVM;
        private ISequenceMediator sequenceMediator;
        private IDeepSkyObject deepSkyObject;
        private IDeepSkyObjectContainer deepSkyObjectContainer;
        private INighttimeCalculator nighttimeCalculator;
        private IProfileService profileService;
        private IApplicationMediator applicationMediator;
        private IPlanetariumFactory planetariumFactory;
        private ICameraMediator cameraMediator;
        private IFilterWheelMediator filterWheelMediator;

        private InputTarget target;




        /// <summary>
        /// The constructor marked with [ImportingConstructor] will be used to import and construct the object
        /// General device interfaces can be added to the constructor parameters and will be automatically injected on instantiation by the plugin loader
        /// </summary>
        /// <remarks>
        /// Available interfaces to be injected:
        ///     - IProfileService,
        ///     - ICameraMediator,
        ///     - ITelescopeMediator,
        ///     - IFocuserMediator,
        ///     - IFilterWheelMediator,
        ///     - IGuiderMediator,
        ///     - IRotatorMediator,
        ///     - IFlatDeviceMediator,
        ///     - IWeatherDataMediator,
        ///     - IImagingMediator,
        ///     - IApplicationStatusMediator,
        ///     - INighttimeCalculator,
        ///     - IPlanetariumFactory,
        ///     - IImageHistoryVM,
        ///     - IDeepSkyObjectSearchVM,
        ///     - IDomeMediator,
        ///     - IImageSaveMediator,
        ///     - ISwitchMediator,
        ///     - ISafetyMonitorMediator,
        ///     - IApplicationMediator
        ///     - IApplicationResourceDictionary
        ///     - IFramingAssistantVM
        ///     - IList<IDateTimeProvider>
        /// </remarks>
        [ImportingConstructor]
        public ToshootInstruction(IFramingAssistantVM framingAssistantVM, ISequenceMediator sequenceMediator, INighttimeCalculator nighttimeCalculator, IProfileService profileService, IApplicationMediator applicationMediator, IPlanetariumFactory planetariumFactory, ICameraMediator cameraMediator, IFilterWheelMediator filterWheelMediator) {
            this.framingAssistantVM = framingAssistantVM;
            this.sequenceMediator = sequenceMediator;
            this.nighttimeCalculator = nighttimeCalculator;
            Text = Properties.Settings.Default.DefaultNotificationMessage;
            this.profileService = profileService;   
            this.applicationMediator = applicationMediator;
            this.planetariumFactory = planetariumFactory;
            this.cameraMediator = cameraMediator;
            this.filterWheelMediator = filterWheelMediator;
                        
        }
        public ToshootInstruction(ToshootInstruction copyMe) : this(copyMe.framingAssistantVM, copyMe.sequenceMediator, copyMe.nighttimeCalculator, copyMe.profileService, copyMe.applicationMediator, copyMe.planetariumFactory, copyMe.cameraMediator, copyMe.filterWheelMediator) {
            CopyMetaData(copyMe);
        }

        
               
        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        [JsonProperty]
        public string Text { get; set; }
        



        /// <summary>
        /// The core logic when the sequence item is running resides here
        /// Add whatever action is necessary
        /// </summary>
        /// <param name="progress">The application status progress that can be sent back during execution</param>
        /// <param name="token">When a cancel signal is triggered from outside, this token can be used to register to it or check if it is cancelled</param>
        /// <returns></returns>
        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            Notification.ShowSuccess(Text);
            // Add logic to run the item here            
            // Crée le dossier pour enregistrer le fichier fini
            string folderPath = Text + "ShootOK";
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
                Console.WriteLine(folderPath);
            }

            string directoryPath = Text;
            string[] files = null;

            // Attends jusqu'à ce qu'un fichier "toconf*.txt" apparaisse dans le répertoire spécifié
            while (files == null || files.Length == 0) {
                Console.WriteLine("En attente d'un fichier toconf*.txt dans le répertoire " + directoryPath);
                System.Threading.Thread.Sleep(1000); // Attend 1 seconde avant de vérifier à nouveau
                files = Directory.GetFiles(directoryPath, "toconf*.txt");
            }

            string closestFile = null;
            int closestNumber = int.MaxValue;

            foreach (string file in files) {
                int number = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(6));
                int difference = Math.Abs(number);
                if (difference < closestNumber) {
                    closestFile = file;
                    closestNumber = difference;
                }

                try {
                    // Ouvrir le fichier en lecture
                    string directdoss = closestFile;
                    StreamReader fichier = new StreamReader(directdoss);
                    // Lire une ligne de texte depuis le fichier
                    string ligne = fichier.ReadLine();

                    // Découper la ligne en utilisant la méthode Split
                    string[] param = ligne.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // effacer les champs 
                                     

                    //nom du champ
                    string namech = param[0];

                    //coordonnées RA champ 
                    int RAh = int.Parse(param[4]);
                    int RAm = int.Parse(param[5]);
                    double RAs = double.Parse(param[6]);

                    //convertis les champs en une valeur degree
                    double ra = (RAh + (RAm / 60.0) + (RAs / 3600.0)) * 15.0;

                    //renvoie l'angle en degree'
                    Angle raok = Angle.ByDegree(ra);

                    //coordonnées DEC champ
                    int DECd = int.Parse(param[7]);
                    int DECm = int.Parse(param[8]);
                    double DECs = double.Parse(param[9]);

                    //recherche le signe devant la dec + ou -
                    double signe = Math.Sign(DECd);

                    //convertis les champs en une valeur en degree
                    double dec = signe * (Math.Abs(DECd) + (DECm / 60.0) + (DECs / 3600.0));

                    //renvoie l'angle en degree
                    Angle decok = Angle.ByDegree(dec);


                    //renvoie les coordonness ra + dec
                    Coordinates coords = new Coordinates(raok, decok, Epoch.J2000);

                    //format pour sequenceur simple
                    IDeepSkyObject deepSkyObject = new DeepSkyObject(Name = namech, coords, null, null);
                                        
                    //envoie dans sequenceur simple
                    //sequenceMediator.AddSimpleTarget(deepSkyObject);

                    double rotation = 0;

                                        

                    IDeepSkyObjectContainer dsoContainer = new DeepSkyObjectContainer(profileService, nighttimeCalculator, framingAssistantVM, applicationMediator, planetariumFactory, cameraMediator, filterWheelMediator);


                    dsoContainer.Target = new InputTarget(Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Latitude), Angle.ByDegree(profileService.ActiveProfile.AstrometrySettings.Longitude), profileService.ActiveProfile.AstrometrySettings.Horizon) {
                        TargetName = namech,
                        InputCoordinates = new InputCoordinates() {
                            Coordinates = coords
                        },
                        Rotation = rotation,



                    };


                    
                    

                    //sequenceMediator.AddAdvancedTarget(deepSkyObjectContainer);



                    //crée le fichier text de suivi de la soirée
                    string fileName = namech + ".txt";
                    File.WriteAllText(Text + "ShootOK/" + fileName, namech);

                    // Fermer le fichier
                    fichier.Close();

                    // Supprimer le fichier
                    if (File.Exists(directdoss)) {
                        File.Delete(directdoss);
                    }
                    break;
                } finally {
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        /// 

        

        public override object Clone() {  
             
            return new ToshootInstruction(this);
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(ToshootInstruction)}, Text: {Text}";
        }
    }
}
