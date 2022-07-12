using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MemoRandom.Client.ViewModels
{
    public class HumanDetailedViewModel : BindableBase
    {
        #region Private Fields
        private readonly IMemoRandomDbController _dbController;
        private string _lastName = "Введите фамилию";
        private string _firstName = "Введите имя";
        private string _patronymic = "Введите отчество";
        private DateTime _birthDate = DateTime.Now;
        private string _birthCountry = "Введите страну рождения";
        private string _birthPlace = "Введите место рождения";
        private DateTime _deathDate = DateTime.Now;
        private string _deathCountry = "Введите страну смерти";
        private string _deathPlace = "Введите место смерти";
        private Guid _deathReasonId;
        private Image _image;
        #endregion

        #region PROPS
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                RaisePropertyChanged();
            }
        }
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged();
            }
        }
        public string Patronymic
        {
            get => _patronymic;
            set
            {
                _patronymic = value;
                RaisePropertyChanged();
            }
        }
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                RaisePropertyChanged();
            }
        }
        public string BirthCountry
        {
            get => _birthCountry;
            set
            {
                _birthCountry = value;
                RaisePropertyChanged();
            }
        }
        public string BirthPlace
        {
            get => _birthPlace;
            set
            {
                _birthPlace = value;
                RaisePropertyChanged();
            }
        }
        public DateTime DeathDate
        {
            get => _deathDate;
            set
            {
                _deathDate = value;
                RaisePropertyChanged();
            }
        }
        public string DeathCountry
        {
            get => _deathCountry;
            set
            {
                _deathCountry = value;
                RaisePropertyChanged();
            }
        }
        public string DeathPlace
        {
            get => _deathPlace;
            set
            {
                _deathPlace = value;
                RaisePropertyChanged();
            }
        }

        public Guid DeathReasonId
        {
            get => _deathReasonId;
            set
            {
                _deathReasonId = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Элемент управления - для изображения
        /// </summary>
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                RaisePropertyChanged(nameof(Image));
            }
        }
        #endregion

        #region COMMANDS
        public DelegateCommand<object> OnDetailedViewLoadedCommand { get; private set; }
        public DelegateCommand SaveHumanCommand { get; private set; }
        public DelegateCommand ImageLoadCommand { get; private set; }
        #endregion

        #region COMMANDS IMPLEMENTATION

        private void OnDetailedViewLoaded(object parameter)
        {
            if (parameter as Image != null)
            {
                Image = parameter as Image;
            }
        }

        /// <summary>
        /// Сохранение данных по человеку
        /// </summary>
        private void SaveHuman()
        {
            // Пока для нового хьюмана
            Human human = new()
            {
                HumanId = Guid.NewGuid(),
                LastName = LastName,
                FirstName = FirstName,
                Patronymic = Patronymic,
                BirthDate = BirthDate,
                BirthCountry = BirthCountry,
                BirthPlace = BirthPlace,
                DeathDate = DeathDate,
                DeathCountry = DeathCountry,
                DeathPlace = DeathPlace,
                HumanImage = ConvertFromBitmapSource((BitmapSource)Image.Source),
                DeathReasonId = DeathReasonId
            };

            _dbController.AddHumanToList(human);
        }

        public static BitmapSource Tempo;

        private byte[] ConvertFromBitmapSource(BitmapSource src)
        {
            byte[] bit;
            //BitmapSource temp = (BitmapSource)Image.Source;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(src));
                encoder.Save(stream);
                bit = stream.ToArray();
                stream.Close();
            }

            return bit;
            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //MemoryStream memoryStream = new MemoryStream();
            //BitmapImage bImg = new BitmapImage();

            //encoder.Frames.Add(BitmapFrame.Create(src));
            //encoder.Save(memoryStream);

            //memoryStream.Position = 0;
            //bImg.BeginInit();
            //bImg.StreamSource = memoryStream;
            //bImg.EndInit();

            //memoryStream.Close();

            //return bImg;
        }

        private void ImageLoad()
        {
            if (Clipboard.ContainsImage())
            {
                Image.Source = Clipboard.GetImage();
                //Tempo = ConvertBitmapSource(Clipboard.GetImage());
            }
            else
            {
                MessageBox.Show("Not an image!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
                //MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
                //BitmapImage bmp = new BitmapImage();
                //bmp.BeginInit();
                //bmp.StreamSource = ms;
                //bmp.UriSource = new Uri("file");
                //bmp.EndInit();
                //img.Source = bmp;
        }
        #endregion

        private void InitializeCommands()
        {
            OnDetailedViewLoadedCommand = new DelegateCommand<object>(OnDetailedViewLoaded);
            SaveHumanCommand = new DelegateCommand(SaveHuman);
            ImageLoadCommand = new DelegateCommand(ImageLoad);
        }

        #region CTOR
        public HumanDetailedViewModel(IMemoRandomDbController dbController)
        {
            _dbController = dbController;

            InitializeCommands();
        }
        #endregion
    }
}
