using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MemoRandom.Client.ViewModels
{
    public class HumanDetailesViewModel : BindableBase
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
        public DelegateCommand SaveHumanCommand { get; private set; }
        public DelegateCommand<object> OnPasteCommand { get; private set; }
        #endregion

        #region COMMANDS IMPLEMENTATION
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
                DeathReasonId = DeathReasonId
            };

            _dbController.AddHumanToList(human);
        }

        private void OnPaste(object param)
        {
            var img = param as Image;
            if (img != null)
            {
                MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.UriSource = new Uri("file");
                bmp.EndInit();
                img.Source = bmp;
            }
        }
        #endregion

        private void InitializeCommands()
        {
            SaveHumanCommand = new DelegateCommand(SaveHuman);
            OnPasteCommand = new DelegateCommand<object>(OnPaste);
        }

        #region CTOR
        public HumanDetailesViewModel(IMemoRandomDbController dbController)
        {
            _dbController = dbController;

            InitializeCommands();
        }
        #endregion
    }
}
