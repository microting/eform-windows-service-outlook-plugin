
using Microting.OutlookAddon.Messages;
using System.Threading.Tasks;
using Rebus.Handlers;
using System;
using eFormCore;
using OutlookOfficeOnline;
using OutlookSql;
using System.Collections.Generic;
using WindowsServiceOutlookPlugin;

namespace Microting.OutlookAddon.Handlers
{
    public class AppointmentCreatedInOutlookHandler : IHandleMessages<AppointmentCreatedInOutlook>
    {
        private readonly SqlController sqlController;
        private readonly Log log;
        private readonly eFormCore.Core sdkCore;
        private readonly IOutlookOnlineController outlookOnlineController;

        public AppointmentCreatedInOutlookHandler(SqlController sqlController, Log log, eFormCore.Core sdkCore, IOutlookOnlineController outlookOnlineController)
        {
            this.sqlController = sqlController;
            this.log = log;
            this.sdkCore = sdkCore;
            this.outlookOnlineController = outlookOnlineController;

        }

#pragma warning disable 1998
        public async Task Handle(AppointmentCreatedInOutlook message)
        {
            try
            {

                Appointment Appo =  sqlController.AppointmentsFind(message.Appo.GlobalId);

                if (Appo.ProcessingState == ProcessingStateOptions.Processed.ToString())
                {
                    bool sqlUpdate = sqlController.AppointmentsUpdate(Appo.GlobalId, ProcessingStateOptions.Created, Appo.Body, "", "", Appo.Completed, Appo.Start, Appo.End, Appo.Duration);
                    if (sqlUpdate)
                    {
                        bool updateStatus = outlookOnlineController.CalendarItemUpdate(Appo.GlobalId, (DateTime)Appo.Start, ProcessingStateOptions.Created, Appo.Body);
                    }
                    else
                    {
                        throw new Exception("Unable to update Appointment in AppointmentCreatedInOutlookHandler");
                    }
                    //log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L623");
                    
                }

                eFormData.MainElement mainElement = sdkCore.TemplateRead((int)Appo.TemplateId);
                if (mainElement == null)
                {
                    log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L625 mainElement is NULL!!!");
                }

                mainElement.Repeated = 1;
                DateTime startDt = new DateTime(Appo.Start.Year, Appo.Start.Month, Appo.Start.Day, 0, 0, 0);
                DateTime endDt = new DateTime(Appo.End.AddDays(1).Year, Appo.End.AddDays(1).Month, Appo.End.AddDays(1).Day, 23, 59, 59);
                //mainElement.StartDate = ((DateTime)appo.Start).ToUniversalTime();
                mainElement.StartDate = startDt;
                //mainElement.EndDate = ((DateTime)appo.End.AddDays(1)).ToUniversalTime();
                mainElement.EndDate = endDt;
                //log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L629");

                #region fill eForm
                if (Appo.AppointmentPrefillFieldValues.Count > 0)
                {
                    SetDefaultValue(mainElement.ElementList, Appo.AppointmentPrefillFieldValues);
                    string description = "";
                    foreach (AppointmentPrefillFieldValue pfv in Appo.AppointmentPrefillFieldValues)
                    {
                        eFormData.Field field = sdkCore.Advanced_FieldRead(pfv.FieldId);
                        string fieldValue = pfv.FieldValue;
                        if (field.FieldType == eFormShared.Constants.FieldTypes.EntitySelect)
                        {
                            fieldValue = sdkCore.EntityItemReadByMicrotingUUID(pfv.FieldValue).Name;
                        }
                            description = description + "<b>" + field.Label + ":</b> " + fieldValue + "<br>";
                    }
                    eFormShared.CDataValue cDataValue = new eFormShared.CDataValue();
                    cDataValue.InderValue = description;
                    mainElement.ElementList[0].Description = cDataValue;
                }
                #endregion


                log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() StartDate is " + mainElement.StartDate);
                log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() EndDate is " + mainElement.EndDate);
                bool allGood = false;
                    List<AppoinntmentSite> appoSites = Appo.AppointmentSites;
                    if (appoSites == null)
                    {
                        log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L635 appoSites is NULL!!! for appo.GlobalId" + Appo.GlobalId);
                    }
                    else
                    {
                        foreach (AppoinntmentSite appo_site in appoSites)
                        {
                            log.LogEverything("Not Specified", "outlookController.foreach AppoinntmentSite appo_site is : " + appo_site.MicrotingSiteUid);
                            string resultId;
                            if (appo_site.MicrotingUuId == null) {
                                 resultId = sdkCore.CaseCreate(mainElement, "", appo_site.MicrotingSiteUid);
                                log.LogEverything("Not Specified", "outlookController.foreach resultId is : " + resultId);
                                
                            } else
                            {
                                resultId = appo_site.MicrotingUuId;
                            }
                            
                            if (!string.IsNullOrEmpty(resultId))
                            {
                            int localCaseId = (int)sdkCore.CaseLookupMUId(resultId).CaseId;
                            //appo_site.MicrotingUuId = resultId;
                            sqlController.AppointmentSiteUpdate((int)appo_site.Id, localCaseId.ToString(), ProcessingStateOptions.Sent);
                                allGood = true;
                            }
                            else
                            {
                                log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L656");
                                allGood = false;
                            }
                        }

                        if (allGood)
                        {
                            //log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L663");
                            bool updateStatus = outlookOnlineController.CalendarItemUpdate(Appo.GlobalId, (DateTime)Appo.Start, ProcessingStateOptions.Sent, Appo.Body);
                            if (updateStatus)
                            {
                                //log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L667");
                                sqlController.AppointmentsUpdate(Appo.GlobalId, ProcessingStateOptions.Sent, Appo.Body, "", "", Appo.Completed, Appo.Start, Appo.End, Appo.Duration);
                            }
                        }
                        else
                        {
                            //log.LogEverything("Not Specified", "outlookController.SyncAppointmentsToSdk() L673");
                            //syncAppointmentsToSdkRunning = false;
                        }
                    }
                //}                
            }
            catch (Exception ex)
            {
                log.LogEverything("Exception", "Got the following exception : " + ex.Message + " and stacktrace is : " + ex.StackTrace);
                throw ex;
            }

        }

        private void SetDefaultValue(List<eFormData.Element> elementLst, List<AppointmentPrefillFieldValue> fieldValues)
        {
            foreach (eFormData.Element element in elementLst)
            {
                if (element.GetType() == typeof(eFormData.DataElement))
                {
                    eFormData.DataElement dataE = (eFormData.DataElement)element;
                    foreach (eFormData.DataItem item in dataE.DataItemList)
                    {
                        if (item.GetType() == typeof(eFormData.NumberStepper))
                        {
                            eFormData.NumberStepper numberStepper = (eFormData.NumberStepper)item;
                            foreach (AppointmentPrefillFieldValue fv in fieldValues)
                            {
                                if (fv.FieldId == item.Id)
                                {
                                    numberStepper.DefaultValue = int.Parse(fv.FieldValue);
                                }
                            }

                        }
                        if (item.GetType() == typeof(eFormData.Number))
                        {
                            eFormData.Number numberStepper = (eFormData.Number)item;
                            foreach (AppointmentPrefillFieldValue fv in fieldValues)
                            {
                                if (fv.FieldId == item.Id)
                                {
                                    numberStepper.DefaultValue = int.Parse(fv.FieldValue);
                                }
                            }

                        }
                        if (item.GetType() == typeof(eFormData.Comment))
                        {
                            eFormData.Comment comment = (eFormData.Comment)item;
                            foreach (AppointmentPrefillFieldValue fv in fieldValues)
                            {
                                if (fv.FieldId == item.Id)
                                {
                                    comment.Value = fv.FieldValue;
                                }
                            }

                        }
                        if (item.GetType() == typeof(eFormData.Text))
                        {
                            eFormData.Text text = (eFormData.Text)item;
                            foreach (AppointmentPrefillFieldValue fv in fieldValues)
                            {
                                if (fv.FieldId == item.Id)
                                {
                                    text.Value = fv.FieldValue;
                                }
                            }

                        }
                        if (item.GetType() == typeof(eFormData.None))
                        {
                            eFormData.None text = (eFormData.None)item;
                            foreach (AppointmentPrefillFieldValue fv in fieldValues)
                            {
                                if (fv.FieldId == item.Id)
                                {
                                    eFormShared.CDataValue cDataValue = new eFormShared.CDataValue();
                                    cDataValue.InderValue = fv.FieldValue;
                                    text.Description = cDataValue;
                                }
                            }

                        }
                        if (item.GetType() == typeof(eFormData.EntitySearch))
                        {
                            eFormData.EntitySearch text = (eFormData.EntitySearch)item;
                            foreach (AppointmentPrefillFieldValue fv in fieldValues)
                            {
                                if (fv.FieldId == item.Id)
                                {
                                    text.DefaultValue = int.Parse(fv.FieldValue);
                                }
                            }
                        }
                        if (item.GetType() == typeof(eFormData.EntitySelect))
                        {
                            eFormData.EntitySelect text = (eFormData.EntitySelect)item;
                            foreach (AppointmentPrefillFieldValue fv in fieldValues)
                            {
                                if (fv.FieldId == item.Id)
                                {
                                    text.DefaultValue = int.Parse(fv.FieldValue);
                                }
                            }
                        }
                    }
                }
                else
                {
                    eFormData.GroupElement groupElement = (eFormData.GroupElement)element;
                    SetDefaultValue(groupElement.ElementList, fieldValues);
                }
            }
        }
    }
}
