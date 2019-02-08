﻿using System;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal class TranslationContext : ITranslationContext
   {
      private Action<string[]> _complete;
      private Action<string, Exception> _fail;

      public TranslationContext(
         string[] untranslatedTexts,
         string sourceLanguage,
         string destinationLanguage,
         Action<string[]> complete,
         Action<string, Exception> fail )
      {
         UntranslatedTexts = untranslatedTexts;
         SourceLanguage = sourceLanguage;
         DestinationLanguage = destinationLanguage;

         _complete = complete;
         _fail = fail;
      }

      public string UntranslatedText => UntranslatedTexts[ 0 ];
      public string[] UntranslatedTexts { get; }
      public string SourceLanguage { get; }
      public string DestinationLanguage { get; }

      internal bool IsDone { get; private set; }


      public void Complete( string translatedText )
      {
         Complete( new[] { translatedText } );
      }

      public void Complete( string[] translatedTexts )
      {
         if( IsDone ) return;

         try
         {
            if( translatedTexts.Length == 0 )
            {
               _fail( "Received empty translation from translator.", null );
               return;
            }

            for( int i = 0 ; i < translatedTexts.Length ; i++ )
            {
               var translatedText = translatedTexts[ 0 ];
               if( string.IsNullOrEmpty( translatedText ) )
               {
                  _fail( "Received empty translation from translator.", null );
                  return;
               }
            }

            _complete( translatedTexts );
         }
         finally
         {
            IsDone = true;
         }
      }

      public void Fail( string reason, Exception exception )
      {
         if( IsDone ) return;

         try
         {
            _fail( reason, exception );

            throw new TranslationContextException();
         }
         finally
         {
            IsDone = true;
         }
      }

      public void Fail( string reason )
      {
         if( IsDone ) return;

         try
         {
            _fail( reason, null );

            throw new TranslationContextException();
         }
         finally
         {
            IsDone = true;
         }
      }

      internal void FailWithoutThrowing( string reason, Exception exception )
      {
         if( IsDone ) return;

         try
         {
            _fail( reason, exception );
         }
         finally
         {
            IsDone = true;
         }
      }

      internal void FailIfNotCompleted()
      {
         if( !IsDone )
         {
            FailWithoutThrowing( "The translation request was not completed before returning from translator.", null );
         }
      }
   }


   [Serializable]
   internal class TranslationContextException : Exception
   {
      public TranslationContextException() { }
      public TranslationContextException( string message ) : base( message ) { }
      public TranslationContextException( string message, Exception inner ) : base( message, inner ) { }
      protected TranslationContextException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
   }
}
