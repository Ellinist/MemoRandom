using System;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using NLog;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Models;

namespace MemoRandom.Client.ViewModels
{
    public class ReasonsViewModel : BindableBase
    {
        #region CONSTANTS
        private const string CHANGE_BUTTON = "Изменить";
        private const string ADD_BUTTON = "Добавить";
        private const string SAVE_BUTTON = "Сохранить";
        #endregion

        #region PRIVATE FIELDS
        private string   _reasonsViewTitle = "Справочник причин смерти";
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IEventAggregator _eventAggregator;
        private readonly ICommonDataController _commonDataController;
        private bool     _cancelButtonEnabled     = false; // Для кнопки отмены
        private bool     _deleteButtonEnabled     = false; // Для кнопки удаления
        private bool     _changeSaveButtonEnabled = false; // Для кнопки изменения/сохранения
        private bool     _addSaveButtonEnabled    = true;  // Для кнопки добавления/сохранения
        private bool     _fieldsEnabled           = false; // Для доступности полей окна
        private bool     _changeMode              = false; // Флаг редактирования записи
        private bool     _addMode                 = false; // Флаг добавления записи
        private string _saveButtonText            = CHANGE_BUTTON;
        private string _addButtonText             = ADD_BUTTON;
        private string _reasonName;
        private string _reasonComment;
        private string _reasonDescription;
        private Reason _selectedReason;
        private bool _transferFlag = false;
        private Reason _transferredReason;
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна справочника причин смерти
        /// </summary>
        public string ReasonsViewTitle
        {
            get => _reasonsViewTitle;
            set
            {
                _reasonsViewTitle = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Свойство доступности кнопки отмены
        /// </summary>
        public bool CancelButtonEnabled
        {
            get => _cancelButtonEnabled;
            set
            {
                _cancelButtonEnabled = value;
                RaisePropertyChanged(nameof(CancelButtonEnabled));
            }
        }
        
        /// <summary>
        /// Свойство доступности кнопки удаления
        /// </summary>
        public bool DeleteButtonEnabled
        {
            get => _deleteButtonEnabled;
            set
            {
                _deleteButtonEnabled = value;
                RaisePropertyChanged(nameof(DeleteButtonEnabled));
            }
        }
        
        /// <summary>
        /// Свойство доступности кнопки изменения/сохранения
        /// </summary>
        public bool ChangeSaveButtonEnabled
        {
            get => _changeSaveButtonEnabled;
            set
            {
                _changeSaveButtonEnabled = value;
                RaisePropertyChanged(nameof(ChangeSaveButtonEnabled));
            }
        }
        
        /// <summary>
        /// Свойство доступности кнопки добавления/сохранения
        /// </summary>
        public bool AddSaveButtonEnabled
        {
            get => _addSaveButtonEnabled;
            set
            {
                _addSaveButtonEnabled = value;
                RaisePropertyChanged(nameof(AddSaveButtonEnabled));
            }
        }
        
        /// <summary>
        /// Текст кнопки изменения/сохранения
        /// </summary>
        public string SaveButtonText
        {
            get => _saveButtonText;
            set
            {
                _saveButtonText = value;
                RaisePropertyChanged(nameof(SaveButtonText));
            }
        }
        
        /// <summary>
        /// Текст кнопки добавления/сохранения
        /// </summary>
        public string AddButtonText
        {
            get => _addButtonText;
            set
            {
                _addButtonText = value;
                RaisePropertyChanged(nameof(AddButtonText));
            }
        }
        
        /// <summary>
        /// Свойство доступности полей для редактирования
        /// </summary>
        public bool FieldsEnabled
        {
            get => _fieldsEnabled;
            set
            {
                _fieldsEnabled = value;
                RaisePropertyChanged(nameof(FieldsEnabled));
            }
        }
        
        /// <summary>
        /// Название причины смерти - является именем узла в иерархическом дереве
        /// </summary>
        public string ReasonName
        {
            get => _reasonName;
            set
            {
                _reasonName = value;
                RaisePropertyChanged(nameof(ReasonName));
            }
        }
        
        /// <summary>
        /// Комментарий к причине смерти
        /// </summary>
        public string ReasonComment
        {
            get => _reasonComment;
            set
            {
                if (_reasonComment == value) return;
                _reasonComment = value;
                RaisePropertyChanged(nameof(ReasonComment));
            }
        }
        
        /// <summary>
        /// Описание причины смерти
        /// </summary>
        public string ReasonDescription
        {
            get => _reasonDescription;
            set
            {
                if (_reasonDescription == value) return;
                _reasonDescription = value;
                RaisePropertyChanged(nameof(ReasonDescription));
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
        /// Коллекция причин смерти - для древовидного отображения
        /// </summary>
        public ObservableCollection<Reason> ReasonsCollection
        {
            get => CommonDataController.ReasonsCollection;
            set
            {
                if (CommonDataController.ReasonsCollection == value) return;
                CommonDataController.ReasonsCollection = value;
                RaisePropertyChanged(nameof(ReasonsCollection));
            }
        }

        /// <summary>
        /// Плоский список - нужен только для работы с БД и со списком в окне редактирования людей
        /// </summary>
        private List<Reason> PlainReasonsList
        {
            get => CommonDataController.PlainReasonsList;
            set
            {
                CommonDataController.PlainReasonsList = value;
                RaisePropertyChanged(nameof(PlainReasonsList));
            }
        }

        public Visibility InformationVisibility { get; set; } = Visibility.Hidden;
        #endregion

        #region COMMANDS
        /// <summary>
        ///Команда внесения причины смерти в иерархическое дерево
        /// </summary>
        public DelegateCommand<object> InsertCommand { get; private set; }
        /// <summary>
        /// Команда изменения выбранной причины смерти в иерархическом дереве
        /// </summary>
        public DelegateCommand ChangeCommand { get; private set; }
        /// <summary>
        /// Команда выбора элемента в иерархическом дереве причин смерти
        /// </summary>
        public DelegateCommand<object> SelectNodeCommand { get; private set; }
        /// <summary>
        /// Команда удаления узла (причины смерти)
        /// </summary>
        public DelegateCommand<object> DeleteNodeCommand { get; private set; }
        /// <summary>
        /// Команда отмены внесенных изменений
        /// </summary>
        public DelegateCommand<object> CancelCommand { get; private set; }
        /// <summary>
        /// Команда клика на пустом месте TreeView (отмена выбора узла)
        /// </summary>
        public DelegateCommand EmptyClickCommand { get; private set; }
        /// <summary>
        /// Команда записи (обновления) данных
        /// </summary>
        public DelegateCommand UpdateCommand { get; private set; }
        /// <summary>
        /// Команда получения данных
        /// </summary>
        public DelegateCommand LoadCommand { get; private set; }
        /// <summary>
        /// Команда переноса узла со всеми дочерними узлами
        /// </summary>
        public DelegateCommand<object> MoveNodeCommand { get; private set; }
        #endregion
        
        #region Блок отработки команд
        /// <summary>
        /// Перенос узла в дереве причин смерти
        /// </summary>
        /// <param name="obj"></param>
        private void OnMoveNodeCommand(object obj)
        {
            if(obj == null)
            {
                MessageBox.Show("Узел для переноса не выбран", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            else
            {
                InformationVisibility = Visibility.Visible;
                RaisePropertyChanged(nameof(InformationVisibility));
                _transferFlag = true; // Да, еще видимость этой кнопки также надо сделать управляемой

                _transferredReason = obj as Reason;

                CancelButtonEnabled     = false; // Кнопка отмены недоступна
                ChangeSaveButtonEnabled = false; // Кнопка редактирования/сохранения недоступна
                AddSaveButtonEnabled    = false; // Кнопка добавления/изменения недоступна
                DeleteButtonEnabled     = false;
            }
        }

        /// <summary>
        /// Команда добавления записи в справочник причин смерти
        /// </summary>
        private async void OnInsertCommand(object obj)
        {
            if (!_addMode) // Заход в блок внесения изменений в поля окна
            {
                SelectedReason          = obj as Reason;
                CancelButtonEnabled     = true;       // Кнопка отмены разблокирована - на случай, если передумали
                ChangeSaveButtonEnabled = false;      // В режиме добавления кнопка редактирования недоступна
                DeleteButtonEnabled     = false;      // В режиме редактирования кнопка удаления недоступна
                AddButtonText           = SAVE_BUTTON;

                FieldsEnabled = true;
                _addMode = true;
            }
            else // Занесение изменений в иерархическое дерево
            {
                if (string.IsNullOrEmpty(ReasonName))
                {
                    MessageBox.Show("Нельзя создать причину смерти с пустым названием!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Создаем новый экземпляр причины смерти
                Reason rsn = new()
                {
                    ReasonId          = Guid.NewGuid(), // Регистрация нового идентификатора
                    ReasonName        = ReasonName,
                    ReasonComment     = ReasonComment ?? string.Empty,
                    ReasonDescription = ReasonDescription ?? string.Empty
                };
                if (SelectedReason != null) // Если узел выбран, то создаем дочку
                {
                    rsn.ReasonParentId = SelectedReason.ReasonId;
                    rsn.ReasonParent   = SelectedReason;
                    SelectedReason     = rsn;
                    SelectedReason.ReasonChildren.Add(rsn);
                    PlainReasonsList.Add(rsn);
                }
                else
                {
                    rsn.ReasonParentId = Guid.Empty;
                    SelectedReason     = rsn;
                    ReasonsCollection.Add(rsn); // Если узел не выбран, то создаем в корне
                    PlainReasonsList.Add(rsn);
                }

                _commonDataController.UpdateHierarchicalReasonsData();
                
                await Task.Run(() =>
                {
                    var result = _commonDataController.AddReasonToFile(rsn);
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (!result)
                        {
                            MessageBox.Show("Не удалось сохранить причину", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        FieldsEnabled           = false; // Делаем поля недоступными - пока не выберем узел и не выберем команду
                        CancelButtonEnabled     = false; // После добавления новой записи кнопка отмены недоступна
                        ChangeSaveButtonEnabled = false; // После добавления новой записи кнопка изменения недоступна до выбора узла
                        DeleteButtonEnabled     = false; // После добавления новой записи кнопка удаления недоступна до выбора узла
                        AddButtonText           = ADD_BUTTON; // Возвращаем название кнопки

                        SelectedReason.IsSelected = true;
                        RaisePropertyChanged(nameof(SelectedReason));
                        _addMode = false;
                    });
                });

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Команда внесения в иерархическое дерево произведенных изменений
        /// </summary>
        private async void OnChangeCommand()
        {
            if (_changeMode) // Если в режиме редактирования - завершение
            {
                var currentReason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == SelectedReason.ReasonId);
                SelectedReason.ReasonName        = ReasonName;
                if (currentReason != null)
                {
                    currentReason.ReasonName = ReasonName;
                    SelectedReason.ReasonComment = ReasonComment;
                    currentReason.ReasonComment = ReasonComment;
                    SelectedReason.ReasonDescription = ReasonDescription;
                    currentReason.ReasonDescription = ReasonDescription;
                }

                FieldsEnabled           = false;
                SaveButtonText          = CHANGE_BUTTON;
                _changeMode             = false; // Флаг, что не в режиме редактирования
                AddSaveButtonEnabled    = true;  // При выходе из режима редактирования кнопка добавления становится доступной
                ChangeSaveButtonEnabled = true;  
                DeleteButtonEnabled     = false; // После изменения записи кнопка удаления недоступна до выбора узла
                CancelButtonEnabled     = false; // После изменения записи кнопка отмены недоступна до выбора узла

                await Task.Run(() =>
                {
                    var result = _commonDataController.ChangeReasonInFile(SelectedReason);

                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (!result)
                        {
                            MessageBox.Show("Не удалось обновить данные!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                });

                RaisePropertyChanged(nameof(ReasonsCollection));
            }
            else // Вход в режим редактирования
            {
                AddSaveButtonEnabled = false;      // При входе в режим редактирования кнопка добавления недоступна
                DeleteButtonEnabled  = false;      // При входе в режим редактирования кнопка удаления недоступна
                CancelButtonEnabled  = true;       // В режиме редактирования кнопка отмены доступна
                FieldsEnabled        = true;       // Поля становятся доступными
                SaveButtonText       = SAVE_BUTTON; // Меняем название кнопки
                _changeMode          = true;       // Флаг, что в режиме редактирования
            }
        }

        /// <summary>
        /// Команда выбора элемента в иерархическом дереве причин смерти
        /// </summary>
        /// <param name="obj"></param>
        private async void OnSelectNodeCommand(object obj)
        {
            if (_transferFlag) // Перенос узла
            {
                if (obj == null) return;

                var reason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == _transferredReason.ReasonId);

                bool matchFlag = false; // Флаг нахождения недопустимой ситуации

                OnceAgain(obj as Reason);

                void OnceAgain(Reason rsn) // Рекурсивная проверка узлов
                {
                    if(rsn.ReasonParent != null)
                    {
                        if (rsn.ReasonParent.ReasonId == _transferredReason.ReasonId) // Ужасное совпадение
                        {
                            MessageBox.Show("Нельзя родительскую причину перенести в ее дочернюю!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                            matchFlag = true;
                        }
                        else
                        {
                            OnceAgain(rsn.ReasonParent);
                        }
                    }
                    else
                    {
                        if (matchFlag) return;

                        reason.ReasonParent = obj as Reason;
                        reason.ReasonParentId = (obj as Reason).ReasonId;
                    }
                }

                ReasonsCollection.Clear();
                _commonDataController.FormObservableCollection(PlainReasonsList, null);
                RaisePropertyChanged(nameof(ReasonsCollection));

                await Task.Run(() =>
                {
                    var result = _commonDataController.SaveReasons();

                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (!result)
                        {
                            MessageBox.Show("Не удалось обновить данные!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                });


                InformationVisibility = Visibility.Hidden;
                RaisePropertyChanged(nameof(InformationVisibility));
                _transferFlag = false;
                //TODO надо правильно расставлять приоритеты
                //CancelButtonEnabled     = true; // Кнопка отмены недоступна
                //ChangeSaveButtonEnabled = true; // Кнопка редактирования/сохранения недоступна
                //AddSaveButtonEnabled = true; // Кнопка добавления/изменения недоступна
                //DeleteButtonEnabled     = true;
            }
            else // Обычная работа (без переноса узлов)
            {
                if (obj == null)
                {
                    DeleteButtonEnabled     = false; // При нулевом узле удалять нечего - кнопка недоступна
                    ChangeSaveButtonEnabled = false; // При нулевом узле нечего редактировать - кнопка недоступна
                    SetEmptyFields(); // Очищаем поля окна
                }
                else
                {
                    DeleteButtonEnabled     = true; // Узел выбран - кнопка удаления доступна
                    ChangeSaveButtonEnabled = true; // Узел выбран - кнопка редактирования/сохранения доступна
                    SelectedReason = obj as Reason;
                    ReasonName = SelectedReason?.ReasonName;
                    ReasonComment = SelectedReason?.ReasonComment;
                    ReasonDescription = SelectedReason?.ReasonDescription;
                }
            }
        }

        /// <summary>
        /// Команда удаления узла (причины смерти)
        /// При этом автоматом удаляются все дочерние узлы
        /// Подумать, не вставить ли туда предупреждение об этом
        /// </summary>
        /// <param name="obj"></param>
        private async void OnDeleteNodeCommand(object obj)
        {
            if (obj is not Reason selectedNode) return;

            if (selectedNode.ReasonParent != null)
            {
                selectedNode.ReasonParent.ReasonChildren.Remove(selectedNode.ReasonParent.ReasonChildren.First(x => x.ReasonId == selectedNode.ReasonId));
            }
            else
            {
                ReasonsCollection.Remove(selectedNode);
            }

            var currentReason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == selectedNode.ReasonId);
            PlainReasonsList.Remove(currentReason);

            await Task.Run(() =>
            {
                List<Guid> result = new List<Guid>();
                DeletingDaughters(selectedNode, result);

                var res = _commonDataController.DeleteReasonAndDaughtersInFile(result);
                if (!res)
                {
                    MessageBox.Show("Не удалось удалить причину!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            RaisePropertyChanged(nameof(ReasonsCollection));

            SelectedReason.IsSelected = false;
            SetEmptyFields(); // Очищаем поля окна
            DeleteButtonEnabled = false; // Удаление выполнено - кнопка удаления становится недоступной
            ChangeSaveButtonEnabled = false; // Удаление выполнено - кнопка редактирования становится недоступной

        }
        
        /// <summary>
        /// Команда отмены внесенных для узла изменений
        /// </summary>
        /// <param name="obj"></param>
        private void OnCancelCommand(object obj)
        {
            SelectedReason            = obj as Reason;
            ReasonName                = SelectedReason?.ReasonName;
            ReasonComment             = SelectedReason?.ReasonComment;
            ReasonDescription         = SelectedReason?.ReasonDescription;

            SelectedReason.IsSelected = true;
            CancelButtonEnabled       = false; // После отмены кнопка становится недоступной
            AddSaveButtonEnabled      = true; // После отмены кнопка добавления становится доступной
            ChangeSaveButtonEnabled   = false; // После отмены кнопка изменения становится недоступной

            #region Все названия кнопок возвращаются в свои первоначальные состояния
            AddButtonText  = ADD_BUTTON;
            SaveButtonText = CHANGE_BUTTON;
            #endregion

            FieldsEnabled = false;
            _addMode      = false;
            _changeMode   = false;
        }
        
        /// <summary>
        /// Команда обработки снятия выделения узла при клике на пустом месте TreeView
        /// </summary>
        private async void OnEmptyClickCommand()
        {
            if (_transferFlag) // Перенос узла в корень
            {
                var reason = PlainReasonsList.FirstOrDefault(x => x.ReasonId == _transferredReason.ReasonId);

                reason.ReasonParent = null;
                reason.ReasonParentId = Guid.Empty;

                ReasonsCollection.Clear();
                _commonDataController.FormObservableCollection(PlainReasonsList, null);
                RaisePropertyChanged(nameof(ReasonsCollection));

                await Task.Run(() =>
                {
                    var result = _commonDataController.SaveReasons();

                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (!result)
                        {
                            MessageBox.Show("Не удалось обновить данные!", "Memo-Random!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                });

                InformationVisibility = Visibility.Hidden;
                RaisePropertyChanged(nameof(InformationVisibility));
                _transferFlag = false;
                //TODO надо правильно расставлять приоритеты
                //CancelButtonEnabled     = true; // Кнопка отмены недоступна
                //ChangeSaveButtonEnabled = true; // Кнопка редактирования/сохранения недоступна
                //DeleteButtonEnabled     = true;
                AddSaveButtonEnabled = true; // Кнопка добавления/изменения недоступна
            }
            else // Обычный клик на пустом месте
            {
                CancelButtonEnabled     = false; // Кнопка отмены недоступна - нечего отменять
                ChangeSaveButtonEnabled = false; // Кнопка редактирования/сохранения недоступна - нечего редактировать

                if (SelectedReason == null) return;

                SelectedReason.IsSelected = false;
                SelectedReason = null;
            }
        }
        
        /// <summary>
        /// Метод установки пустых полей окна
        /// </summary>
        private void SetEmptyFields()
        {
            ReasonName        = string.Empty;
            ReasonComment     = string.Empty;
            ReasonDescription = string.Empty;
        }
        #endregion

        /// <summary>
        /// Рекурсивный метод удаления дочерних узлов для удаляемой причины смерти
        /// </summary>
        private static void DeletingDaughters(Reason reason, List<Guid> result)
        {
            result.Add(reason.ReasonId); // Заносим ID в список

            foreach (var child in reason.ReasonChildren) // Если есть дочерние узлы, то выполняем удаление и по ним
            {
                DeletingDaughters(child, result);
            }
        }

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            InsertCommand     = new DelegateCommand<object>(OnInsertCommand);
            SelectNodeCommand = new DelegateCommand<object>(OnSelectNodeCommand);
            DeleteNodeCommand = new DelegateCommand<object>(OnDeleteNodeCommand);
            CancelCommand     = new DelegateCommand<object>(OnCancelCommand);
            EmptyClickCommand = new DelegateCommand(OnEmptyClickCommand);
            ChangeCommand     = new DelegateCommand(OnChangeCommand);
            MoveNodeCommand   = new DelegateCommand<object>(OnMoveNodeCommand);
        }









        #region CTOR

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="commonDataController"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ReasonsViewModel(ILogger logger,
                                IEventAggregator eventAggregator,
                                ICommonDataController commonDataController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _commonDataController = commonDataController ?? throw new ArgumentNullException(nameof(commonDataController));

            InitializeCommands();
        }
        #endregion
    }
}