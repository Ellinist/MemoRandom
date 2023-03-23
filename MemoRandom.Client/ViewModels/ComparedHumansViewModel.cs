using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Models;
using Prism.Commands;
using Prism.Mvvm;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления для окна добавления люедй для сравнения
    /// </summary>
    public class ComparedHumansViewModel :BindableBase
    {
        #region CONSTANTS
        private const double SOURCE_WIDTH = 450;
        private const double SOURCE_HEIGHT = 350;
        #endregion

        #region PRIVATE FIELDS
        private readonly ICommonDataController _commonDataController;

        private string _comparedHumansTitle = "Люди для сравнения";
        private ObservableCollection<ComparedHuman> _comparedHumansCollection;
        private Guid _comparedHumanId;
        private string _comparedHumanFullName;
        private DateTime _comparedHumanBirthDate;
        private int _selectedIndex;
        private ComparedHuman _selectedHuman;
        private bool _isConsidered;
        private bool _newFlag = false;

        private double _left = 0; // Левый верхний угол изображения на канве (координата X)
        private double _top = 0; // Левый верхний угол изображения на канве (координата Y)
        private double _scaleX = 1; // Масштаб по оси X
        private double _scaleY = 1; // Масштаб по оси Y

        private double _startX; // Начальное значение X левого верхнего угла
        private double _startY; // Начальное значение Y левого верхнего угла
        private double _deltaX; // Смещение изображения по оси X по отношению к холсту
        private double _deltaY; // Смещение изображения по оси Y по отношению к холсту
        private double _shiftX; // Сдвиг картинки по оси X при ее перемещении
        private double _shiftY; // Сдвиг картинки по оси Y при ее перемещении
        private BitmapSource _sourceImageSource;
        private BitmapSource _targetImageSource;

        private Canvas _canvas;
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна
        /// </summary>
        public string ComparedHumansTitle
        {
            get => _comparedHumansTitle;
            set
            {
                _comparedHumansTitle = value;
                RaisePropertyChanged(nameof(ComparedHumansTitle));
            }
        }

        /// <summary>
        /// Коллекция людей для сравнения
        /// </summary>
        public ObservableCollection<ComparedHuman> ComparedHumansCollection
        {
            get => _comparedHumansCollection;
            set
            {
                _comparedHumansCollection = value;
                RaisePropertyChanged(nameof(ComparedHumansCollection));
            }
        }

        /// <summary>
        /// Индекс выбранного человека для сравнения
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(nameof(SelectedIndex));
            }
        }

        /// <summary>
        /// Выбранный человек
        /// </summary>
        public ComparedHuman SelectedHuman
        {
            get => _selectedHuman;
            set
            {
                if (value == null) return;
                _selectedHuman = value;

                ComparedHumanId = SelectedHuman.ComparedHumanId;
                ComparedHumanFullName = SelectedHuman.ComparedHumanFullName;
                ComparedHumanBirthDate = SelectedHuman.ComparedHumanBirthDate;
                SourceImageSource = (BitmapSource)_commonDataController.GetComparedHumanImage(SelectedHuman);
                IsConsidered = SelectedHuman.IsComparedHumanConsidered;
                RaisePropertyChanged(nameof(SelectedHuman));
            }
        }

        /// <summary>
        /// Идентификатор человека для сравнения
        /// </summary>
        public Guid ComparedHumanId
        {
            get => _comparedHumanId;
            set
            {
                _comparedHumanId = value;
                RaisePropertyChanged(nameof(ComparedHumanId));
            }
        }

        /// <summary>
        /// Полное имя человека для сравнения
        /// </summary>
        public string ComparedHumanFullName
        {
            get => _comparedHumanFullName;
            set
            {
                _comparedHumanFullName = value;
                RaisePropertyChanged(nameof(ComparedHumanFullName));
            }
        }

        /// <summary>
        /// Дата рождения человека для сравнения
        /// </summary>
        public DateTime ComparedHumanBirthDate
        {
            get => _comparedHumanBirthDate;
            set
            {
                _comparedHumanBirthDate = value;
                RaisePropertyChanged(nameof(ComparedHumanBirthDate));
            }
        }

        /// <summary>
        /// Рассматривается ли человек для сравнения в прогрессе анализа
        /// </summary>
        public bool IsConsidered
        {
            get => _isConsidered;
            set
            {
                _isConsidered = value;
                RaisePropertyChanged(nameof(IsConsidered));
            }
        }

        /// <summary>
        /// Изображение человека для сравнения
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

        #region COMMANDS
        /// <summary>
        /// Команда добавления человека для сравнения
        /// </summary>
        public DelegateCommand NewComparedHumanCommand { get; private set; }

        /// <summary>
        /// Команда сохранения человека для сравнения во внешнем хранилище
        /// </summary>
        public DelegateCommand<object> SaveComparedHumanCommand { get; private set; }

        /// <summary>
        /// Команда удаления выбранного человека для сравнения
        /// </summary>
        public DelegateCommand DeleteComparedHumanCommand { get; private set; }

        /// <summary>
        /// Команда загрузки изображения
        /// </summary>
        public DelegateCommand ImageLoadCommand { get; private set; }
        #endregion



        /// <summary>
        /// Команда добавления нового человека для сравнения
        /// </summary>
        private void NewComparedHuman()
        {
            _newFlag = true;
            ComparedHumanId = Guid.NewGuid();
            ComparedHumanFullName = "Введите полное имя";
            ComparedHumanBirthDate = DateTime.Now.AddYears(-50);
        }

        /// <summary>
        /// Команда сохранения человека для сравнения во внешнем хранилище
        /// </summary>
        private async void SaveComparedHuman(object obj)
        {
            SetTargetImage(obj);

            var image = BitmapSourceToBitmapImage(TargetImageSource);
            if (!_newFlag) // Существующая запись человека дял сравнения
            {
                #region Обновление выбранного для сравнения человека
                SelectedHuman.ComparedHumanFullName     = ComparedHumanFullName;
                SelectedHuman.ComparedHumanBirthDate    = ComparedHumanBirthDate;
                SelectedHuman.ImageFile = TargetImageSource != null ? SelectedHuman.ComparedHumanId.ToString() + ".jpg" : string.Empty;
                SelectedHuman.IsComparedHumanConsidered = IsConsidered;
                #endregion

                await Task.Run(() =>
                {
                    var result = _commonDataController.UpdateComparedHuman(SelectedHuman, image);
                    if (result) return;

                    MessageBox.Show("Не удалось обновить человека для сравнения", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            else // Создание новой записи человека для сравнения
            {
                ComparedHuman compHuman = new()
                {
                    ComparedHumanId           = ComparedHumanId,
                    ComparedHumanFullName     = ComparedHumanFullName,
                    ComparedHumanBirthDate    = ComparedHumanBirthDate,
                    ImageFile                 = TargetImageSource != null ? ComparedHumanId.ToString() + ".jpg" : string.Empty,
                    IsComparedHumanConsidered = IsConsidered
                };

                await Task.Run(() =>
                {
                    var result = _commonDataController.UpdateComparedHuman(compHuman, image);
                    if (result) return;

                    MessageBox.Show("Не удалось добавить человека для сравнения!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                });

                CommonDataController.ComparedHumansCollection.Add(compHuman);
                RaisePropertyChanged(nameof(ComparedHumansCollection));
                SelectedIndex = ComparedHumansCollection.IndexOf(compHuman);
            }

            _newFlag = false;
        }

        /// <summary>
        /// Команда удаления выбранного человека для сравнения
        /// </summary>
        private void DeleteComparedHuman()
        {
            var result = _commonDataController.DeleteComparedHuman(SelectedHuman.ComparedHumanId);
            if (!result)
            {
                MessageBox.Show("Не удалось удалить человека для сравнения!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CommonDataController.ComparedHumansCollection.Remove(SelectedHuman);
            RaisePropertyChanged(nameof(ComparedHumansCollection));
            SelectedIndex = 0;
        }

        /// <summary>
        /// Обработчик загрузки окна работы с людьми для сравнения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComparedHumansView_Loaded(object sender, RoutedEventArgs e)
        {
            ComparedHumansCollection = CommonDataController.ComparedHumansCollection;
            SelectedIndex = 0;

            SourceImageSource = (BitmapSource)_commonDataController.GetComparedHumanImage(CommonDataController.ComparedHumansCollection[0]); // Загружаем изображение
            RaisePropertyChanged(nameof(ComparedHumansCollection));
        }

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
        /// Обработчик движения мыши над исходным изображением
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PersonImage_MouseMove(object sender, MouseEventArgs e)
        {
            var obj = (sender as Image);
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (obj == null) return;

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

                Left = (SOURCE_WIDTH - (SourceImageSource.Width) * ScaleX) / 2 + _shiftX;
                Top = (SOURCE_HEIGHT - SourceImageSource.Height * ScaleY) / 2 + _shiftY;
            }
            else
            {
                ScaleX += 0.01;
                ScaleY += 0.01;

                Left = (SOURCE_WIDTH - (SourceImageSource.Width) * ScaleX) / 2 + _shiftX;
                Top = (SOURCE_HEIGHT - SourceImageSource.Height * ScaleY) / 2 + _shiftY;
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
                ScaleX = 1;
                ScaleY = 1;
                Left = -(SourceImageSource.Width - SOURCE_WIDTH) / 2;
                Top = -(SourceImageSource.Height - SOURCE_HEIGHT) / 2;
            }
            else
            {
                MessageBox.Show("Неверный формат в буфере обмена!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
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

            MemoryStream ms = new();
            BmpBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(src));
            encoder.Save(ms);
            ms.Position = 0;

            BitmapImage myBitmapImage = new();
            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = ms;
            myBitmapImage.EndInit();
            return myBitmapImage;
        }
        #endregion


        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitCommands()
        {
            NewComparedHumanCommand    = new DelegateCommand(NewComparedHuman);
            SaveComparedHumanCommand   = new DelegateCommand<object>(SaveComparedHuman);
            DeleteComparedHumanCommand = new DelegateCommand(DeleteComparedHuman);
            ImageLoadCommand = new DelegateCommand(ImageLoadFromClipboard);
        }






        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="commonDataController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ComparedHumansViewModel(ICommonDataController commonDataController)
        {
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

            InitCommands();
        }
        #endregion
    }
}
