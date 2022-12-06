using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Models;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления формирования нового человека
    /// </summary>
    public class HumanDetailedViewModel : BindableBase
    {
        public Action CloseAction { get; set; }

        #region PRIVATE FIELDS
        private readonly ILogger          _logger;
        private readonly ICommonDataController _commonDataController;

        private const double SourceWidth = 450;
        private const double SourceHeight = 350;

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
        private Reason   _selectedReason;
        private BitmapSource _targetImageSource;
        private BitmapSource _sourceImageSource;
        private string _humanComments;
        private int _daysLived;
        private double _fullYearsLived;
        private string _humanDeathReasonName;
        private bool _openComboState = false; // По умолчанию комбобокс свернут
        private double _left   = 0; // Левый верхний угол изображения на канве (координата X)
        private double _top    = 0; // Левый верхний угол изображения на канве (координата Y)
        private double _scaleX = 1; // Масштаб по оси X
        private double _scaleY = 1; // Масштаб по оси Y

        private double _startX; // Начальное значение X левого верхнего угла
        private double _startY; // Начальное значение Y левого верхнего угла
        private double _deltaX; // Смещение изображения по оси X по отношению к холсту
        private double _deltaY; // Смещение изображения по оси Y по отношению к холсту
        private double _shiftX; // Сдвиг картинки по оси X при ее перемещении
        private double _shiftY; // Сдвиг картинки по оси Y при ее перемещении

        private Canvas _canvas;
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
        /// Свойство - итоговое изображение
        /// </summary>
        public BitmapSource TargetImageSource
        {
            get => _targetImageSource;
            set
            {
                _targetImageSource = value;
                RaisePropertyChanged(nameof(TargetImageSource));
            }
        }

        /// <summary>
        /// Свойство - исходное изображение
        /// </summary>
        public BitmapSource SourceImageSource
        {
            get => _sourceImageSource;
            set
            {
                _sourceImageSource = value;
                RaisePropertyChanged(nameof(SourceImageSource));
            }
        }

        /// <summary>
        /// Расширенный комментарий
        /// </summary>
        public string HumanComments
        {
            get => _humanComments;
            set
            {
                _humanComments = value;
                RaisePropertyChanged(nameof(HumanComments));
            }
        }

        /// <summary>
        /// Число полных прожитых дней
        /// </summary>
        public int DaysLived
        {
            get => _daysLived;
            set
            {
                _daysLived = value;
                RaisePropertyChanged(nameof(DaysLived));
            }
        }

        /// <summary>
        /// Число полных прожитых лет
        /// </summary>
        public double FullYearsLived
        {
            get => _fullYearsLived;
            set
            {
                _fullYearsLived = value;
                RaisePropertyChanged(nameof(FullYearsLived));
            }
        }

        /// <summary>
        /// Иерархическая коллекция справочника причин смерти
        /// </summary>
        public ObservableCollection<Reason> ReasonsList
        {
            get => CommonDataController.ReasonsCollection;
            set
            {
                CommonDataController.ReasonsCollection = value;
                RaisePropertyChanged(nameof(ReasonsList));
            }
        }

        /// <summary>
        /// Плоский список справочника причин смерти для вытягивания названия причины
        /// </summary>
        public List<Reason> PlainReasonsList
        {
            get => CommonDataController.PlainReasonsList;
            set
            {
                CommonDataController.PlainReasonsList = value;
                RaisePropertyChanged(nameof(PlainReasonsList));
            }
        }

        /// <summary>
        /// Отображаемая причина смерти
        /// </summary>
        public string HumanDeathReasonName
        {
            get => _humanDeathReasonName;
            set
            {
                _humanDeathReasonName = value;
                RaisePropertyChanged(nameof(HumanDeathReasonName));
            }
        }

        /// <summary>
        /// Состояние выпадающего окна с TreeView - развернуто или свернуто
        /// </summary>
        public bool OpenComboState
        {
            get => _openComboState;
            set
            {
                _openComboState = value;
                RaisePropertyChanged(nameof(OpenComboState));
            }
        }

        /// <summary>
        /// Выбранный узел (причина смерти) в иерархическом дереве
        /// </summary>
        public Reason SelectedReason
        {
            get => _selectedReason;
            set
            {
                if (_selectedReason == value) return;
                _selectedReason = value;
                RaisePropertyChanged(nameof(SelectedReason));
            }
        }

        /// <summary>
        /// Координата X верхнего левого угла изображения относительно контейнера-канвы
        /// </summary>
        public double Left
        {
            get => _left;
            set
            {
                _left = value;
                RaisePropertyChanged(nameof(Left));
            }
        }

        /// <summary>
        /// Координата Y верхнего левого угла изображения относительно контейнера-канвы
        /// </summary>
        public double Top
        {
            get => _top;
            set
            {
                _top = value;
                RaisePropertyChanged(nameof(Top));
            }
        }

        /// <summary>
        /// Коэффициент масштабирования по оси X
        /// </summary>
        public double ScaleX
        {
            get => _scaleX;
            set
            {
                _scaleX = value;
                RaisePropertyChanged(nameof(ScaleX));
            }
        }

        /// <summary>
        /// Коэффициент масштабирования по оси Y
        /// </summary>
        public double ScaleY
        {
            get => _scaleY;
            set
            {
                _scaleY = value;
                RaisePropertyChanged(nameof(ScaleY));
            }
        }

        /// <summary>
        /// Холст, в котором расположено изображение
        /// </summary>
        public Canvas SourceCanvas
        {
            get => _canvas;
            set
            {
                _canvas = value;
                RaisePropertyChanged(nameof(SourceCanvas));
            }
        }
        #endregion

        #region Блок работы с изображением
        /// <summary>
        /// Копирование визуального изображения канвы в результирующий контрол
        /// </summary>
        /// <param name="obj"></param>
        private void SetTargetImage(object obj)
        {
            if (obj is not Canvas Canv) return;
            var target = new RenderTargetBitmap((int)(Canv.RenderSize.Width), (int)(Canv.RenderSize.Height), 96, 96, PixelFormats.Pbgra32);
            var brush = new VisualBrush(Canv);

            var visual = new DrawingVisual();
            var drawingContext = visual.RenderOpen();


            drawingContext.DrawRectangle(brush, null, new Rect(new Point(0, 0), new Point(Canv.RenderSize.Width, Canv.RenderSize.Height)));

            drawingContext.Close();

            target.Render(visual);

            TargetImageSource = target;
        }

        /// <summary>
        /// Обработчик нажатия левой кнопки мыши на исходном изображении
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PersonImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image obj)
            {
                Point t2 = obj.PointToScreen(Mouse.GetPosition(obj));
                _startX = t2.X;
                _startY = t2.Y;
            }
        }

        /// <summary>
        /// Обработчик движения мыши над исходным изображением
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PersonImage_MouseMove(object sender, MouseEventArgs e)
        {
            var obj = (sender as Image);
            if (e.LeftButton != MouseButtonState.Pressed) return;
            Point t = obj.PointToScreen(Mouse.GetPosition(obj));

            var currentX = t.X;
            var currentY = t.Y;

            _deltaX = currentX - _startX;
            _deltaY = currentY - _startY;

            _shiftX += _deltaX;
            _shiftY += _deltaY;

            Left += _deltaX;
            Top += _deltaY;

            _startX = currentX;
            _startY = currentY;
        }

        /// <summary>
        /// Обработчик прокрутки колесом мыши над исходным изображением
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SourceCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                ScaleX -= 0.01;
                ScaleY -= 0.01;

                Left = (SourceWidth - (SourceImageSource.Width) * ScaleX) / 2 + _shiftX;
                Top = (SourceHeight - SourceImageSource.Height * ScaleY) / 2 + _shiftY;
            }
            else
            {
                ScaleX += 0.01;
                ScaleY += 0.01;

                Left = (SourceWidth - (SourceImageSource.Width) * ScaleX) / 2 + _shiftX;
                Top = (SourceHeight - SourceImageSource.Height * ScaleY) / 2 + _shiftY;
            }
        }
        #endregion

        #region COMMANDS
        /// <summary>
        /// Команда сохранения данных по человеку
        /// </summary>
        public DelegateCommand SaveHumanCommand { get; private set; }
        
        /// <summary>
        /// Команда загрузки изображения
        /// </summary>
        public DelegateCommand ImageLoadCommand { get; private set; }

        /// <summary>
        /// Команда выбора узла с иерархическом дереве причин смерти
        /// </summary>
        public DelegateCommand<object> SelectNodeCommand { get; private set; }

        public DelegateCommand<object> SetTargetImageCommand { get; private set; }
        #endregion

        #region COMMANDS IMPLEMENTATION
        /// <summary>
        /// Загрузка окна добавления/редактирования человека
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DetailedView_Loaded(object sender, RoutedEventArgs e)
        {
            Human human = CommonDataController.CurrentHuman;

            if (human != null)
            {
                LastName          = human.LastName;
                FirstName         = human.FirstName;
                Patronymic        = human.Patronymic;
                BirthDate         = human.BirthDate;
                BirthCountry      = human.BirthCountry;
                BirthPlace        = human.BirthPlace;
                DeathDate         = human.DeathDate;
                DeathCountry      = human.DeathCountry;
                DeathPlace        = human.DeathPlace;
                HumanComments     = human.HumanComments;
                DeathReasonId     = human.DeathReasonId;
                TargetImageSource = (BitmapSource)_commonDataController.GetHumanImage(CommonDataController.CurrentHuman); // Загружаем изображение
            }
            else // Создание нового человека
            {
                TimeSpan ts = TimeSpan.FromDays(365 * 50);
                LastName      = "Введите фамилию";
                FirstName     = "Введите имя";
                Patronymic    = "Введите отчество";
                BirthDate     = DateTime.Now - ts;
                BirthCountry  = "Введите страну рождения";
                BirthPlace    = "Введите место рождения";
                DeathDate     = DateTime.Now;
                DeathCountry  = "Введите страну смерти";
                DeathPlace    = "Введите место смерти";
                HumanComments = "Введите краткое описание";
            }
            ReasonsList      = CommonDataController.ReasonsCollection;
            PlainReasonsList = CommonDataController.PlainReasonsList;

            var t = PlainReasonsList.Find(x => x.ReasonId == DeathReasonId);
            if (t != null)
            {
                HumanDeathReasonName = t.ReasonName;
            }
            RaisePropertyChanged(nameof(ReasonsList));
            RaisePropertyChanged(nameof(HumanDeathReasonName));
        }

        /// <summary>
        /// Сохранение данных по человеку
        /// </summary>
        private async void SaveHuman()
        {
            var curHuman = CommonDataController.CurrentHuman;

            (int years, int days) = _commonDataController.GetYearsAndDaysConsideredLeaps(BirthDate, DeathDate);

            if (curHuman != null) // Редактирование
            {
                curHuman.LastName       = LastName;
                curHuman.FirstName      = FirstName;
                curHuman.Patronymic     = Patronymic;
                curHuman.BirthDate      = BirthDate;
                curHuman.BirthCountry   = BirthCountry;
                curHuman.BirthPlace     = BirthPlace;
                curHuman.DeathDate      = DeathDate;
                curHuman.DeathCountry   = DeathCountry;
                curHuman.DeathPlace     = DeathPlace;
                curHuman.ImageFile      = TargetImageSource != null ? curHuman.HumanId.ToString() + ".jpg" : string.Empty;
                curHuman.HumanComments  = HumanComments;
                curHuman.DeathReasonId  = DeathReasonId;
                curHuman.DaysLived      = (DeathDate - BirthDate).TotalDays; // Считаем полное число прожитых дней
                curHuman.FullYearsLived = years;

                CommonDataController.CurrentHuman = curHuman;
            }
            else // Добавление нового
            {
                var newHumanId = Guid.NewGuid();
                Human human = new()
                {
                    HumanId        = newHumanId,
                    LastName       = LastName,
                    FirstName      = FirstName,
                    Patronymic     = Patronymic,
                    BirthDate      = BirthDate,
                    BirthCountry   = BirthCountry,
                    BirthPlace     = BirthPlace,
                    DeathDate      = DeathDate,
                    DeathCountry   = DeathCountry,
                    DeathPlace     = DeathPlace,
                    ImageFile      = TargetImageSource != null ? newHumanId.ToString() + ".jpg" : string.Empty,
                    HumanComments  = HumanComments,
                    DeathReasonId  = DeathReasonId,
                    DaysLived      = (DeathDate - BirthDate).TotalDays, // Считаем полное число прожитых дней
                    FullYearsLived = years
                };

                CommonDataController.CurrentHuman = human;

                //CommonDataController.HumansList.Add(human); // И добавляем в основной список
            }

            bool result = true;
            try
            {
                var image = BitmapSourceToBitmapImage(TargetImageSource);
                await Task.Run(() =>
                {
                    result = _commonDataController.UpdateHumanInRepository(CommonDataController.CurrentHuman, image);
                });
                if(result) CloseAction(); // Закрываем окно
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить сохранение! Код ошибки в журнале!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                _logger.Error($"Ошибка: {ex}");
            }
        }

        /// <summary>
        /// Преобразование экранного изображения в BitmapImage
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private BitmapImage BitmapSourceToBitmapImage(BitmapSource src)
        {
            if (src == null) return null;

            MemoryStream ms = new MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(src));
            encoder.Save(ms);
            ms.Position = 0;

            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = ms;
            myBitmapImage.EndInit();
            return myBitmapImage;
        }

        /// <summary>
        /// Щелчок на узле причины смерти
        /// </summary>
        /// <param name="obj"></param>
        private void ClickReasonNode(object obj)
        {
            SelectedReason = obj as Reason;
            if(SelectedReason != null)
            {
                DeathReasonId = SelectedReason.ReasonId;
                HumanDeathReasonName = SelectedReason.ReasonName;
                RaisePropertyChanged(nameof(HumanDeathReasonName));
                OpenComboState = false;
                RaisePropertyChanged(nameof(OpenComboState));
            }
        }

        /// <summary>
        /// Метод загрузки изображения из буфера обмена
        /// </summary>
        private void ImageLoadFromClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                SourceImageSource = Clipboard.GetImage();
                // На случай повторной загрузки нового изображения (если старое не понравилось)
                // Обнуляем сдвиг изображения и устанавливаем масштаб = без изменений
                _shiftX = 0;
                _shiftY = 0;
                ScaleX  = 1;
                ScaleY  = 1;
                Left    = -(SourceImageSource.Width  - SourceWidth)  / 2;
                Top     = -(SourceImageSource.Height - SourceHeight) / 2;
            }
            else
            {
                MessageBox.Show("Попытка впихнуть невпихуемое!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            SaveHumanCommand      = new DelegateCommand(SaveHuman);
            ImageLoadCommand      = new DelegateCommand(ImageLoadFromClipboard);
            SelectNodeCommand     = new DelegateCommand<object>(ClickReasonNode);
            SetTargetImageCommand = new DelegateCommand<object>(SetTargetImage);
        }

        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msSqlController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HumanDetailedViewModel(ILogger logger,
                                      ICommonDataController commonDataController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

            InitializeCommands();
        }
        #endregion
    }
}



///// <summary>
///// Преобразование байтового массива в BitmapImage
///// </summary>
///// <param name="array"></param>
///// <returns></returns>
//private BitmapImage ConvertFromByteArray(byte[] array)
//{
//    if (array == null) return null;

//    BitmapImage myBitmapImage = new BitmapImage();
//    myBitmapImage.BeginInit();
//    myBitmapImage.StreamSource = new MemoryStream(array);
//    myBitmapImage.DecodePixelWidth = 200;
//    myBitmapImage.EndInit();
//    return myBitmapImage;
//}

///// <summary>
///// Преобразование BitmapSource в массив байтов
///// </summary>
///// <param name="src"></param>
///// <returns></returns>
//private byte[] ConvertFromBitmapSource(BitmapSource src)
//{
//    if(src == null) return null;

//    byte[] bit;
//    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
//    encoder.QualityLevel = 100;
//    using (MemoryStream stream = new MemoryStream())
//    {
//        encoder.Frames.Add(BitmapFrame.Create(src));
//        encoder.Save(stream);
//        bit = stream.ToArray();
//        stream.Close();
//    }

//    return bit;
//}
