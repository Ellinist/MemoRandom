using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace MemoRandom.Client.Common.Models
{
    public abstract class Person : BindableBase
    {
        #region PRIVATE FIELDS
        private Guid _personId;
        private DateTime _birthDate;
        private string _imageFile;
        #endregion

        #region PROPS
        public Guid PersonId
        {
            get => _personId;
            set
            {
                _personId = value;
                RaisePropertyChanged(nameof(PersonId));
            }
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                RaisePropertyChanged(nameof(BirthDate));
            }
        }

        public string ImageFile
        {
            get => _imageFile;
            set
            {
                _imageFile = value;
                RaisePropertyChanged(nameof(ImageFile));
            }
        }
        #endregion
    }
}
