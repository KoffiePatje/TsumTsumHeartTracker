using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;

namespace nl.pleduc.TsumTsumHeartTracker
{
    public class NumberPickerPreference : DialogPreference
    {
        public int MinValue { get { return minValue; } set { minValue = value; Value = global::System.Math.Max(minValue, Value); } }
        public int MaxValue { get { return maxValue; } set { maxValue = value; Value = global::System.Math.Min(maxValue, Value); } }
        public int Value { get { return value; } set { this.value = global::System.Math.Min(maxValue, global::System.Math.Max(minValue, value)); } }

        private NumberPicker numberPicker;

        private int minValue = 1;
        private int maxValue = 30;
        private int value = 5;

        public NumberPickerPreference(Context context) : this(context, null) { }

        public NumberPickerPreference(Context context, IAttributeSet attrs) : base(context, attrs) {
            TypedArray a = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.NumberPickerPreference, 0, 0);
            try
            {
                MinValue = a.GetInteger(Resource.Styleable.NumberPickerPreference_minValue, 1);
                MaxValue = a.GetInteger(Resource.Styleable.NumberPickerPreference_maxValue, 30);
            }
            finally
            {
                a.Recycle();
            }

            DialogLayoutResource = Resource.Layout.preference_numberpicker;
            PositiveButtonText = "OK";
            NegativeButtonText = "Cancel";
            DialogIcon = null;
        }

        protected override void OnSetInitialValue(bool restorePersistedValue, Java.Lang.Object defaultValue)
        {
            Value = restorePersistedValue ? GetPersistedInt(5) : (int)defaultValue;
        }

        protected override void OnBindDialogView(View view)
        {
            base.OnBindDialogView(view);

            numberPicker = (NumberPicker)view.FindViewById(Resource.Id.preference_numberpicker);
            numberPicker.MinValue = MinValue;
            numberPicker.MaxValue = MaxValue;
            numberPicker.Value = Value;
        }

        protected override void OnDialogClosed(bool positiveResult)
        {
            base.OnDialogClosed(positiveResult);

            if (positiveResult)
            {
                numberPicker.ClearFocus();
                int numPickerValue = numberPicker.Value;
                if (CallChangeListener(numPickerValue))
                {
                    Value = numPickerValue;
                }
            }
        }

        protected override IParcelable OnSaveInstanceState()
        {
            IParcelable parcable = base.OnSaveInstanceState();

            SavedState savedState = new SavedState(parcable);
            savedState.minValue = MinValue;
            savedState.maxValue = MaxValue;
            savedState.value = Value;

            return savedState;
        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            if(state == null || !(state is SavedState))
            {
                base.OnRestoreInstanceState(state);
                return;
            }
            else
            {
                SavedState savedState = state as SavedState;
                MinValue = savedState.minValue;
                MaxValue = savedState.maxValue;
                Value = savedState.value;
            }
        }

        private class SavedState : BaseSavedState
        {
            public int minValue;
            public int maxValue;
            public int value;

            public SavedState(IParcelable state) : base(state) { }

            public SavedState(Parcel state) : base(state)
            {
                minValue = state.ReadInt();
                maxValue = state.ReadInt();
                value = state.ReadInt();
            }

            public override void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);

                dest.WriteInt(minValue);
                dest.WriteInt(maxValue);
                dest.WriteInt(value);
            }

            [ExportField("CREATOR")]
            public static SavedStateCreator InitializeCreator()
            {
                return new SavedStateCreator();
            }

            public class SavedStateCreator : Java.Lang.Object, IParcelableCreator
            {
                public Java.Lang.Object CreateFromParcel(Parcel source)
                {
                    return new SavedState(source);
                }

                public Java.Lang.Object[] NewArray(int size)
                {
                    return new SavedState[size];
                }
            }
        }
    }
}
