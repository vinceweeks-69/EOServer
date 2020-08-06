using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ViewModels.ControllerModels;
using ViewModels.DataModels;

namespace SharedData
{
    public static class Email
    {
        //https://emailmg.dotster.com/roundcube/?_task=login
        //service@elegantorchids.com
        //Orchids@5185

        public static bool SendEmail(EOMailMessage msg)
        {
            bool success = false;

            SmtpClient smtp = new SmtpClient();

            smtp.Port = 587;
            smtp.Host = "smtp.dotster.com";  //"smtp.gmail.com"; //for gmail host  
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(msg.MailMessage.From.Address, msg.Pwd);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                smtp.Send(msg.MailMessage);
                success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return success;
        }
    }

    public class EmailHelpers
    {
        public string ComposeReceipt(WorkOrderResponse workOrder, WorkOrderPaymentDTO payment)
        {
            string receiptHtml = String.Empty;

            receiptHtml =
                "<div>" +
                    "Order Placed: " + workOrder.WorkOrder.CreateDate.ToShortDateString() + "<br>" +
                    "Order # " + workOrder.WorkOrder.WorkOrderId.ToString() + "<br>" +
                    "Order Total: " + String.Format("Order Total: {0:C}", payment.WorkOrderPaymentAmount) + "<br>" + "<br>" +
                    "<div style='border:1px; solid; black'>" +
                        "<div style='border:1px solid; black; text-align: center'>" +
                            "<div style='display: inline-block'> Order Detail</div>";

            foreach(WorkOrderInventoryMapDTO inventoryMap in workOrder.WorkOrderList)
            {
                receiptHtml += inventoryMap.Quantity.ToString() + " " + inventoryMap.InventoryName + "  " + inventoryMap.Size + "<br>";
            }

            receiptHtml +=
                        "</div>" +
                         "<div style='border:1px solid; black;'>" +
                            "<div>" +
                            "<div>" +
                        "<div>" +
                    "</div>";

            if(workOrder.WorkOrder.DeliveryType == 2)
            {
                receiptHtml += "<br>" +
                 "<div style='border:1px; solid; black'>" +
                    "<div style='border:1px solid; black; text-align: center'>" +
                        "<div style='display: inline-block'> Delivery Detail</div>" +
                    "</div>" +
                    "<div style='border:1px solid; black;'>" +
                        "<div>" +
                        "</div>" +
                    "</div>" +
                "</div>"; 
            }

            receiptHtml += "<br>" +
                 "<div style='border:1px; solid; black'>" +
                    "<div style='border:1px solid; black; text-align: center'>" +
                        "<div style='display: inline-block'> Payment Detail</div>" +
                    "</div>" +
                    "<div style='border:1px solid; black;'>" +
                        "<div>" +
                        "<div>" +
                    "</div>" +
                 "</div>";

            if(payment.WorkOrderPaymentType == 2)
            {
                receiptHtml +=
                 "<div style='border:1px; solid; black'>" +
                    "<div style='border:1px solid; black; text-align: center'>" +
                        "<div style='display: inline-block'> Credit Card Payment Detail</div>" +
                    "</div>" +
                    "<div style='border:1px solid; black;'>" +
                        "<div>" +
                        "<div>" +
                    "</div>" +
                 "</div>";
            }

            receiptHtml += "</div>";

            return receiptHtml;
        }

        public string ComposeMissingEmail(PersonAndAddressDTO buyer)
        {
            string receiptHtml = String.Empty;

            if (String.IsNullOrEmpty(buyer.Person.first_name)) { buyer.Person.first_name = "missing"; }

            if (String.IsNullOrEmpty(buyer.Person.last_name)) { buyer.Person.last_name = "missing"; }

            if (String.IsNullOrEmpty(buyer.Person.street_address)) 
            {
                buyer.Person.street_address = "missing";

                if (buyer.Address != null)
                {
                    if(!String.IsNullOrEmpty(buyer.Address.street_address))
                    {
                        buyer.Person.street_address = buyer.Address.street_address;
                    }
                }
            }

            if (String.IsNullOrEmpty(buyer.Person.city)) 
            { 
                buyer.Person.city = "missing";

                if (buyer.Address != null)
                {
                    if (!String.IsNullOrEmpty(buyer.Address.city))
                    {
                        buyer.Person.city = buyer.Address.city;
                    }
                }
            }

            if (String.IsNullOrEmpty(buyer.Person.zipcode)) 
            { 
                buyer.Person.zipcode = "missing";

                if (buyer.Address != null)
                {
                    if (!String.IsNullOrEmpty(buyer.Address.zipcode))
                    {
                        buyer.Person.zipcode = buyer.Address.zipcode;
                    }
                }
            }

            if (String.IsNullOrEmpty(buyer.Person.phone_primary)) { buyer.Person.phone_primary = "missing"; }

            receiptHtml = "<h1> Customer Missing Email </h1>" +
                "<div> Reach out to this customer and ask them if they'd like to add a valid email address </div>" +
                "<br/>" +
                "<div> Customer Information </div>" +
                "<div> First Name: " + buyer.Person.first_name + "</div>" +
                "<div> Last Name: " + buyer.Person.last_name + "</div>" +
                "<div> Address: " + buyer.Person.street_address + "</div>" +
                "<div> City: " + buyer.Person.city + "</div>" +
                "<div> Zip: " + buyer.Person.zipcode + "</div>" +
                "<div> Phone: " + buyer.Person.phone_primary + "</div>";

            return receiptHtml;
        }

        public string ComposeMissingImage(object DTO)
        {
            string missingImageHtml = "";

            if(DTO as PlantInventoryDTO != null)
            {
                PlantInventoryDTO piDTO = DTO as PlantInventoryDTO;

                missingImageHtml = "<h1> Plant  Missing Image </h1>" +
                "<div> This plant record is missing an image - use the EO Mobile App or EO Admin app to add an appropriate image </div>" +
                "<br/>" +
                "<div> Plant Information </div>" +
                "<div> Plant ID: " + piDTO.Plant.PlantId.ToString() + "</div>" +
                "<div> Plant Name: " + piDTO.Plant.PlantName + "</div>" +
                "<div> Type Name: " + piDTO.Plant.PlantTypeName + "</div>" +
                "<div> Size: " + piDTO.Plant.PlantSize + "</div>";
            }
            else if(DTO as ContainerInventoryDTO != null)
            {
                ContainerInventoryDTO ciDTO = DTO as ContainerInventoryDTO;

                missingImageHtml = "<h1> Container Missing Image </h1>" +
                "<div> This container record is missing an image - use the EO Mobile App or EO Admin app to add an appropriate image </div>" +
                "<br/>" +
                "<div> Container Information </div>" +
                "<div> Container ID: " + ciDTO.Container.ContainerId.ToString() + "</div>" +
                "<div> Plant Name: " + ciDTO.Container.ContainerName + "</div>" +
                "<div> Type Name: " + ciDTO.Container.ContainerTypeName + "</div>" +
                "<div> Size: " + ciDTO.Container.ContainerSize + "</div>";
            }
            else if (DTO as MaterialInventoryDTO != null)
            {
                MaterialInventoryDTO miDTO = DTO as MaterialInventoryDTO;

                missingImageHtml = "<h1> Material Missing Image </h1>" +
                "<div> This material record is missing an image - use the EO Mobile App or EO Admin app to add an appropriate image </div>" +
                "<br/>" +
                "<div> Material Information </div>" +
                "<div> Material ID: " + miDTO.Material.MaterialId.ToString() + "</div>" +
                "<div> Material Name: " + miDTO.Material.MaterialName + "</div>" +
                "<div> Type Name: " + miDTO.Material.MaterialTypeName + "</div>" +
                "<div> Size: " + miDTO.Material.MaterialSize + "</div>";
            }

            else if (DTO as FoliageInventoryDTO != null)
            {
                FoliageInventoryDTO fiDTO = DTO as FoliageInventoryDTO;

                missingImageHtml = "<h1> Foliage Missing Image </h1>" +
                "<div> This foliage record is missing an image - use the EO Mobile App or EO Admin app to add an appropriate image </div>" +
                "<br/>" +
                "<div> Foliage Information </div>" +
                "<div> Foliage ID: " + fiDTO.Foliage.FoliageId.ToString() + "</div>" +
                "<div> Foliage Name: " + fiDTO.Foliage.FoliageName + "</div>" +
                "<div> Type Name: " + fiDTO.Foliage.FoliageTypeName + "</div>" +
                "<div> Size: " + fiDTO.Foliage.FoliageSize + "</div>";
            }

            return missingImageHtml;
        }
    }
}
