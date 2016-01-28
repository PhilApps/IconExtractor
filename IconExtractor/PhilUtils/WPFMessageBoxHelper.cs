using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MsgBox = System.Windows.MessageBox;

namespace PhilUtils.WPF
{
	/// <summary>
	/// <para>Aide pour affichage de messageboxs</para>
	/// <para>V1.1</para>
	/// </summary>
	static class WPFMessageBoxHelper
	{
		private static bool MessageBoxDisplayCondition(Window source)
		{
			return (null != Application.Current && null != Application.Current.Dispatcher
				&& !Application.Current.Dispatcher.HasShutdownStarted
				&& source.IsVisible);
		}
		#region Modèle de messageBox simple
		private static void MsgBoxSimple(Window source, string message, string caption,
			MessageBoxImage messageBoxImage)
		{
			if (!MessageBoxDisplayCondition(source)) return;
			MsgBox.Show(source, message, caption, MessageBoxButton.OK, messageBoxImage);
		}
		private static Task MsgBoxSimpleAsync(this Window source, string message, string caption,
			MessageBoxImage messageBoxImage)
		{
			if (!MessageBoxDisplayCondition(source)) return Task.FromResult<bool>(false);

			Dispatcher disp = source.Dispatcher;
			Action act = () => MsgBox.Show(source, message, caption, MessageBoxButton.OK, messageBoxImage);
			return disp.InvokeAsync(act, DispatcherPriority.Normal).Task;
		}
		#endregion

		#region Messagebox d'info
		internal static void MsgBoxInfo(this Window source, string message, string caption = "")
		{
			MsgBoxSimple(source, message, caption, MessageBoxImage.Information);
		}
		internal static Task MsgBoxInfoAsync(this Window source, string message, string caption = "")
		{
			return MsgBoxSimpleAsync(source, message, caption, MessageBoxImage.Information);
		}
		#endregion

		#region Messagebox warning
		internal static void MsgBoxWarning(this Window source, string message, string caption = "")
		{
			MsgBoxSimple(source, message, caption, MessageBoxImage.Warning);
		}
		internal static Task MsgBoxWarningAsync(this Window source, string message, string caption = "")
		{
			return MsgBoxSimpleAsync(source, message, caption, MessageBoxImage.Warning);
		}
		#endregion

		#region Messagebox erreur
		internal static void MsgBoxError(this Window source, string message, string caption = "")
		{
			MsgBoxSimple(source, message, caption, MessageBoxImage.Error);
		}
		internal static Task MsgBoxErrorAsync(this Window source, string message, string caption = "")
		{
			return MsgBoxSimpleAsync(source, message, caption, MessageBoxImage.Error);
		}
		#endregion

		#region Modèle de messageBox avec résultat

		private abstract class MessageBoxResultToBoolConverter
		{
			private MessageBoxResultToBoolConverter() { }
			internal abstract bool? Convert(MessageBoxResult messageBoxResult);
			internal abstract MessageBoxResult ConvertBack(bool? result, MessageBoxButton msgBoxBtn);

			internal static MessageBoxResultToBoolConverter TwoValsConv
			{ get { return TwoValsToBoolConverter_Class.Instance; } }
			internal static MessageBoxResultToBoolConverter ThreeValsConv
			{ get { return ThreeValsToBoolConverter_Class.Instance; } }

			private sealed class TwoValsToBoolConverter_Class : MessageBoxResultToBoolConverter
			{
				#region Singleton
				private static SpinLock _spinlockInstance = new SpinLock(false);
				private static MessageBoxResultToBoolConverter _instance = null;
				internal static MessageBoxResultToBoolConverter Instance
				{
					get
					{
						if (null == _instance)
						{
							using (_spinlockInstance.GetLock())
							{
								if (null == _instance)
								{
									_instance = new TwoValsToBoolConverter_Class();
								}
							}
						}
						return _instance;
					}
				}
				#endregion

				private TwoValsToBoolConverter_Class() { }
				internal override bool? Convert(MessageBoxResult messageBoxResult)
				{
					switch (messageBoxResult)
					{
						case MessageBoxResult.Cancel:
						case MessageBoxResult.No:
						case MessageBoxResult.None:
							return false;
						case MessageBoxResult.OK:
						case MessageBoxResult.Yes:
							return true;
						default:
							throw new NotSupportedException();
					}
				}

				internal override MessageBoxResult ConvertBack(bool? result, MessageBoxButton msgBoxBtn)
				{
					switch (msgBoxBtn)
					{
						case MessageBoxButton.OKCancel:
							return result.GetValueOrDefault(false) ? MessageBoxResult.OK : MessageBoxResult.Cancel;
						case MessageBoxButton.YesNo:
						case MessageBoxButton.YesNoCancel:
							return result.GetValueOrDefault(false) ? MessageBoxResult.Yes : MessageBoxResult.No;
						default:
							throw new InvalidOperationException();
					}
				}
			}
			private sealed class ThreeValsToBoolConverter_Class : MessageBoxResultToBoolConverter
			{
				#region Singleton
				private static SpinLock _spinlockInstance = new SpinLock(false);
				private static MessageBoxResultToBoolConverter _instance = null;
				internal static MessageBoxResultToBoolConverter Instance
				{
					get
					{
						if (null == _instance)
						{
							using (_spinlockInstance.GetLock())
							{
								if (null == _instance)
								{
									_instance = new ThreeValsToBoolConverter_Class();
								}
							}
						}
						return _instance;
					}
				}
				#endregion

				private ThreeValsToBoolConverter_Class() { }
				internal override bool? Convert(MessageBoxResult messageBoxResult)
				{
					switch (messageBoxResult)
					{
						case MessageBoxResult.Cancel:
							return null;
						case MessageBoxResult.No:
						case MessageBoxResult.None:
							return false;
						case MessageBoxResult.OK:
						case MessageBoxResult.Yes:
							return true;
						default:
							throw new NotSupportedException();
					}
				}

				internal override MessageBoxResult ConvertBack(bool? result, MessageBoxButton msgBoxBtn)
				{
					switch (msgBoxBtn)
					{
						case MessageBoxButton.OKCancel:
							return result.GetValueOrDefault(false) ? MessageBoxResult.OK : MessageBoxResult.Cancel;
						case MessageBoxButton.YesNo:
							return result.GetValueOrDefault(false) ? MessageBoxResult.Yes : MessageBoxResult.No;
						case MessageBoxButton.YesNoCancel:
							return (!result.HasValue) ? MessageBoxResult.Cancel :
								result.Value ? MessageBoxResult.Yes : MessageBoxResult.No;
						default:
							throw new InvalidOperationException();
					}
				}
			}
		}

		private static bool? MsgBoxResult(Window window, string message, string caption,
			MessageBoxButton messageBoxButton, MessageBoxImage icon,
			bool? messageBoxDefaultResult, MessageBoxResultToBoolConverter converter)
		{
			if (!window.IsVisible) return messageBoxDefaultResult;
			Debug.Assert(null != converter);
			return converter.Convert(MsgBox.Show(window, message,
				caption, messageBoxButton, icon, converter.ConvertBack(messageBoxDefaultResult, messageBoxButton)));
		}

		private static Task<bool?> MsgBoxResultAsync(Window source, string message, string caption,
			MessageBoxButton messageBoxButton, MessageBoxImage icon,
			bool? messageBoxDefaultResult, MessageBoxResultToBoolConverter converter)
		{
			if (!source.IsVisible) return Task.FromResult(messageBoxDefaultResult);
			Debug.Assert(null != converter);

			Dispatcher disp = source.Dispatcher;
			Func<bool?> fct = () => converter.Convert(MsgBox.Show(source, message, caption,
				messageBoxButton, icon, converter.ConvertBack(messageBoxDefaultResult, messageBoxButton)));
			return disp.InvokeAsync(fct).Task;
		}
		private static Task<bool> MsgBox2ValsResultAsync(Window source, string message, string caption,
			MessageBoxButton messageBoxButton, MessageBoxImage icon,
			bool messageBoxDefaultResult)
		{
			Task<bool?> tskRes = MsgBoxResultAsync(source, message, caption, messageBoxButton,
				icon, messageBoxDefaultResult, MessageBoxResultToBoolConverter.TwoValsConv);
			return tskRes.ContinueWith(tsk => tsk.Result.Value, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		#endregion

		#region OKCancel

		internal static bool MsgBoxOKCancel(this Window source, string message, string caption = "",
			MessageBoxImage icon = MessageBoxImage.Question, bool messageBoxDefaultResult = false)
		{
			return MsgBoxResult(source, message, caption, MessageBoxButton.OKCancel, icon,
				messageBoxDefaultResult, MessageBoxResultToBoolConverter.TwoValsConv).Value;
		}

		internal static Task<bool> MsgBoxOKCancelAsync(this Window source, string message, string caption = "",
			MessageBoxImage icon = MessageBoxImage.Question, bool messageBoxDefaultResult = false)
		{
			return MsgBox2ValsResultAsync(source, message, caption,
				MessageBoxButton.OKCancel, icon, messageBoxDefaultResult);
		}

		#endregion

		#region YesNo

		internal static bool MsgBoxYesNo(this Window source, string message, string caption = "",
			MessageBoxImage icon = MessageBoxImage.Question, bool messageBoxDefaultResult = false)
		{
			return MsgBoxResult(source, message, caption, MessageBoxButton.YesNo, icon,
				messageBoxDefaultResult, MessageBoxResultToBoolConverter.TwoValsConv).Value;
		}

		internal static Task<bool> MsgBoxYesNoAsync(this Window source, string message, string caption = "",
			MessageBoxImage icon = MessageBoxImage.Question, bool messageBoxDefaultResult = false)
		{
			return MsgBox2ValsResultAsync(source, message, caption,
				MessageBoxButton.YesNo, icon, messageBoxDefaultResult);
		}

		#endregion

		#region YesNoCancel

		internal static bool? MsgBoxYesNoCancel(this Window source, string message, string caption = "",
			MessageBoxImage icon = MessageBoxImage.Question, bool? messageBoxDefaultResult = null)
		{
			return MsgBoxResult(source, message, caption, MessageBoxButton.YesNoCancel, icon,
				messageBoxDefaultResult, MessageBoxResultToBoolConverter.ThreeValsConv).Value;
		}

		internal static Task<bool?> MsgBoxYesNoCancelAsync(this Window source, string message, string caption = "",
			MessageBoxImage icon = MessageBoxImage.Question, bool? messageBoxDefaultResult = null)
		{
			return MsgBoxResultAsync(source, message, caption,
				MessageBoxButton.YesNoCancel, icon, messageBoxDefaultResult,
				MessageBoxResultToBoolConverter.ThreeValsConv);
		}

		#endregion
	}
}
