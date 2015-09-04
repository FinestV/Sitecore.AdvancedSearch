using System;
using System.Collections;
using System.Collections.Generic;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.LuceneProvider;

namespace Sitecore.AdvancedSearch.DocumentBuilder
{
    public class LuceneCachedDocumentBuilder : LuceneDocumentBuilder
    {
        public LuceneCachedDocumentBuilder(IIndexable indexable, IProviderUpdateContext context) : base(indexable, context)
        {
        }

        public override void AddComputedIndexFields()
        {
            var cachedComputedValues = new Dictionary<string, object>();
            object obj;
            try
            {
                VerboseLogging.CrawlingLogDebug(() => "AddComputedIndexFields Start");
                foreach (IComputedIndexField computedIndexField in base.Options.ComputedIndexFields)
                {
                    try
                    {

                        var type = computedIndexField.GetType().FullName;
                        if (cachedComputedValues.ContainsKey(type))
                        {
                            obj = cachedComputedValues[type];
                        }
                        else
                        {
                            obj = computedIndexField.ComputeFieldValue(base.Indexable);
                            cachedComputedValues[type] = obj;   
                        }
                    }
                    catch (Exception exception1)
                    {
                        Exception exception = exception1;
                        if (base.Settings.StopOnCrawlFieldError())
                        {
                            throw;
                        }
                        else
                        {
                            CrawlingLog.Log.Error(string.Format("Could not compute value for ComputedIndexField: {0} for indexable: {1}", computedIndexField.FieldName, base.Indexable.UniqueId), exception);
                            continue;
                        }
                    }
                    LuceneSearchFieldConfiguration fieldConfiguration = base.Index.Configuration.FieldMap.GetFieldConfiguration(computedIndexField.FieldName) as LuceneSearchFieldConfiguration;
                    if (obj is IEnumerable && !(obj is string))
                    {
                        foreach (object obj1 in obj as IEnumerable)
                        {
                            if (fieldConfiguration == null)
                            {
                                this.AddField(computedIndexField.FieldName, obj1, false);
                            }
                            else
                            {
                                this.AddField(computedIndexField.FieldName, obj1, fieldConfiguration, 0f);
                            }
                        }
                    }
                    else if (fieldConfiguration == null)
                    {
                        this.AddField(computedIndexField.FieldName, obj, false);
                    }
                    else
                    {
                        this.AddField(computedIndexField.FieldName, obj, fieldConfiguration, 0f);
                    }
                }
            }
            finally
            {
                VerboseLogging.CrawlingLogDebug(() => "AddComputedIndexFields End");
            }
        }
    }
}
