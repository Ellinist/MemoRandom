using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MemoRandom.Data.Implementations;
using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления формирования нового человека
    /// </summary>
    public class HumanDetailedViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private readonly IHumansController _humanController;

        private string   _lastName;
        private string   _firstName;
        private string   _patronymic;
        private DateTime _birthDate;
        private string   _birthCountry;
        private string   _birthPlace;
        private DateTime _deathDate;
        private string   _deathCountry;
        private string   _deathPlace;
        private Guid     _deathReasonId;
        private Image    _image;
        #endregion

        #region PROPS
        /// <summary>
        /// Фамилия человека
        /// </summary>
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Имя человека
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Отчество человека
        /// </summary>
        public string Patronymic
        {
            get => _patronymic;
            set
            {
                _patronymic = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Страна рождения
        /// </summary>
        public string BirthCountry
        {
            get => _birthCountry;
            set
            {
                _birthCountry = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Место рождения
        /// </summary>
        public string BirthPlace
        {
            get => _birthPlace;
            set
            {
                _birthPlace = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Дата смерти
        /// </summary>
        public DateTime DeathDate
        {
            get => _deathDate;
            set
            {
                _deathDate = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Страна смерти
        /// </summary>
        public string DeathCountry
        {
            get => _deathCountry;
            set
            {
                _deathCountry = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Место смерти
        /// </summary>
        public string DeathPlace
        {
            get => _deathPlace;
            set
            {
                _deathPlace = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Идентификатор причины смерти
        /// </summary>
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
        /// <summary>
        /// Команда загрузки окна детальных данных по человеку
        /// </summary>
        public DelegateCommand<object> OnDetailedViewLoadedCommand { get; private set; }
        
        /// <summary>
        /// Команда сохранения данных по человеку
        /// </summary>
        public DelegateCommand SaveHumanCommand { get; private set; }
        
        /// <summary>
        /// Команда загрузки изображения
        /// </summary>
        public DelegateCommand ImageLoadCommand { get; private set; }
        #endregion

        #region COMMANDS IMPLEMENTATION
        /// <summary>
        /// Метод, выполняемый после загрузки окна
        /// </summary>
        /// <param name="parameter"></param>
        private void OnDetailedViewLoaded(object parameter)
        {
            if (parameter as Image != null)
            {
                Image = parameter as Image;

                Human human = _humanController.GetCurrentHuman();

                if (human != null)
                {
                    LastName = human.LastName;
                    FirstName = human.FirstName;
                    Patronymic = human.Patronymic;
                    BirthDate = human.BirthDate;
                    BirthCountry = human.BirthCountry;
                    BirthPlace = human.BirthPlace;
                    DeathDate = human.DeathDate;
                    DeathCountry = human.DeathCountry;
                    DeathPlace = human.DeathPlace;
                    Image.Source = ConvertFromByteArray(human.HumanImage);
                }
                else
                {
                    LastName = "Введите фамилию";
                    FirstName = "Введите имя";
                    Patronymic = "Введите отчество";
                    BirthDate = DateTime.Now;
                    BirthCountry = "Введите страну рождения";
                    BirthPlace = "Введите место рождения";
                    DeathDate = DateTime.Now;
                    DeathCountry = "Введите страну смерти";
                    DeathPlace = "Введите место смерти";
                }
            }
        }

        /// <summary>
        /// Сохранение данных по человеку
        /// </summary>
        private void SaveHuman()
        {
            var curHuman = _humanController.GetCurrentHuman();
            if (curHuman != null) // Редактирование
            {
                curHuman.LastName = LastName;
                curHuman.FirstName = FirstName;
                curHuman.Patronymic = Patronymic;
                curHuman.BirthDate = BirthDate;
                curHuman.BirthCountry = BirthCountry;
                curHuman.BirthPlace = BirthPlace;
                curHuman.DeathDate = DeathDate;
                curHuman.DeathCountry = DeathCountry;
                curHuman.DeathPlace = DeathPlace;
                curHuman.HumanImage = ConvertFromBitmapSource((BitmapSource)Image.Source);
                curHuman.ImageFilePath = $"./Images/{LastName}";
                curHuman.DeathReasonId = DeathReasonId;

                _humanController.SetCurrentHuman(curHuman);
            }
            else // Добавление нового
            {
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
                    ImageFilePath = $"./Images/{LastName}",
                    DeathReasonId = DeathReasonId
                };

                _humanController.SetCurrentHuman(human);
            }

            _humanController.UpdateHumans();
        }

        private BitmapImage ConvertFromByteArray(byte[] array)
        {
            if (array == null) return null;

            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = new MemoryStream(array);
            myBitmapImage.DecodePixelWidth = 200;
            myBitmapImage.EndInit();
            return myBitmapImage;
        }

        /// <summary>
        /// Преобразование BitmapSource в массив байтов
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private byte[] ConvertFromBitmapSource(BitmapSource src)
        {
            if(src == null) return null;

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
        }

        /// <summary>
        /// Метод загрузки изображения из буфера обмена
        /// </summary>
        private void ImageLoad()
        {
            if (Clipboard.ContainsImage())
            {
                Image.Source = Clipboard.GetImage();
            }
            else
            {
                MessageBox.Show("Not an image!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            OnDetailedViewLoadedCommand = new DelegateCommand<object>(OnDetailedViewLoaded);
            SaveHumanCommand = new DelegateCommand(SaveHuman);
            ImageLoadCommand = new DelegateCommand(ImageLoad);
        }

        #region CTOR
        public HumanDetailedViewModel(IHumansController humansController)
        {
            _humanController = humansController ?? throw new ArgumentNullException(nameof(humansController));

            InitializeCommands();
        }
        #endregion
    }
}
