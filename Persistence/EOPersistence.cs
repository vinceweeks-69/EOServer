using EO.DatabaseContext;
using EO.ViewModels.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Org.BouncyCastle.Asn1.Ocsp;
using Persistence.Helpers;
using SharedData;
//using SharedData.ListFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ViewModels.ControllerModels;
using ViewModels.DataModels;

namespace EO.Persistence
{
    public class EOPersistence : IEOPersistence
    {
        private eotestContext dbContext;


        public EOPersistence()
        {
            //connectionString="server=127.0.0.1;port=3306;user=EOSystem;password=Orchids@5185;database=eotest"
            var optionsBuilder = new DbContextOptionsBuilder<eotestContext>();
            optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=jVW@696969;database=eotest");
            dbContext = new eotestContext(optionsBuilder.Options);
        }

        public LoginDTO GetUser(LoginDTO request)
        {
            LoginDTO loginDTO = new LoginDTO();
            try
            {
                if (request != null)
                {
                    User user = dbContext.User.Where(a => a.UserName == request.UserName && a.Password == request.Password).FirstOrDefault();

                    if (user != null && user.UserId > 0)
                    {
                        loginDTO.UserId = user.UserId;
                        loginDTO.RoleId = user.RoleId;
                    }
                }
            }
            catch (Exception ex)
            {
                //implement logging
            }

            return loginDTO;
        }

        public List<UserDTO> GetUsers()
        {
            List<UserDTO> userDTO = new List<UserDTO>();
            try
            {
                List<User> userList = dbContext.User.ToList();

                foreach(User user in userList)
                {
                    userDTO.Add(new UserDTO()
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        RoleId = user.RoleId
                    });
                }
            }
            catch (Exception ex)
            {
                //implement logging
            }

            return userDTO;
        }

        public byte[] GetImage(long imageId)
        {
            byte[] imageData = new byte[0];

            Image img = dbContext.Image.Where(a => a.ImageId == imageId).FirstOrDefault();

            if (img != null && img.ImageId != 0)
            {
                imageData = img.ImageData;
            }

            return imageData;
        }

       

        public GetContainerTypeResponse GetContainerTypes()
        {
            GetContainerTypeResponse response = new GetContainerTypeResponse();

            List<ContainerTypeDTO> containerTypes = new List<ContainerTypeDTO>();

            dbContext.ContainerType.ToList().ForEach(item =>
            {
                containerTypes.Add(new ContainerTypeDTO()
                {
                    ContainerTypeId = item.ContainerTypeId,
                    ContainerTypeName = item.ContainerTypeName
                });
            });

            response.ContainerTypeList = containerTypes;
                        
            return response;
        }

        public List<ContainerNameDTO> GetContainerNamesByType(long containerTypeId)
        {
            List<ContainerNameDTO> containerNames = new List<ContainerNameDTO>();

            dbContext.ContainerName.Where(a => a.ContainerTypeId == containerTypeId).ToList().ForEach(item =>
            {
                containerNames.Add(new ContainerNameDTO()
                {
                    ContainerNameId = item.ContainerNameId,
                    ContainerTypeId = item.ContainerTypeId,
                    ContainerName = item.Name
                });
            });

            return containerNames;
        }

        public GetContainerResponse GetContainersByType(long containerTypeId)
        {
            GetContainerResponse response = new GetContainerResponse();

            List<long> containerIds = dbContext.Container.Where(b => b.ContainerTypeId == containerTypeId).Select(a => a.ContainerId).ToList();

            List<InventoryContainerMap> inventoryContainerMap = dbContext.InventoryContainerMap.Where(a => containerIds.Contains(a.ContainerId)).ToList();

            List<long> inventoryIds = inventoryContainerMap.Select(a => a.InventoryId).ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.Where(a => inventoryIds.Contains(a.InventoryId)).ToList();

            GetContainerTypeResponse containerTypeList = GetContainerTypes();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            foreach (InventoryContainerMap map in inventoryContainerMap)
            {
                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == map.InventoryId).First();

                Container c = dbContext.Container.Where(b => b.ContainerId == map.ContainerId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    InventoryTypeName = "Plants", // inventoryTypeList.Where(a => a.InventoryTypeId == i.InventoryTypeId).Select(b => b.InventoryTypeName).First(),
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = serviceCodeList.Where(a => a.ServiceCodeId == i.ServiceCodeId).Select(b => b.ServiceCode).First(),
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity
                };

                ContainerDTO cDTO = new ContainerDTO()
                {
                    ContainerId = c.ContainerId,
                    ContainerName = c.ContainerName,
                    ContainerTypeId = c.ContainerTypeId,
                    ContainerTypeName = containerTypeList.ContainerTypeList.Where(a => a.ContainerTypeId == c.ContainerTypeId).Select(b => b.ContainerTypeName).First()
                };

                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                response.ContainerInventoryList.Add(new ContainerInventoryDTO(cDTO, iDTO, imageId));
            }
            return response;
        }

        public GetContainerResponse GetContainer(long containerId)
        {
            GetContainerResponse response = new GetContainerResponse();

            Container c = dbContext.Container.Where(a => a.ContainerId == containerId).First();

            ContainerType type = dbContext.ContainerType.Where(a => a.ContainerTypeId == c.ContainerTypeId).First();

            ContainerName name = dbContext.ContainerName.Where(a => a.ContainerTypeId == c.ContainerTypeId).First();

            ContainerDTO cDTO = new ContainerDTO()
            {
                ContainerId = c.ContainerId,
                ContainerTypeId = c.ContainerTypeId,
                ContainerName = name.Name,
                ContainerTypeName = type.ContainerTypeName
            };


            long inventoryId = dbContext.InventoryContainerMap.Where(a => a.ContainerId == containerId).Select(b => b.InventoryId).First();

            Inventory i = dbContext.Inventory.Where(a => a.InventoryId == inventoryId).First();

            ServiceCode sc = dbContext.ServiceCode.Where(a => a.ServiceCodeId == i.ServiceCodeId).First();

            InventoryDTO iDTO = new InventoryDTO()
            {
                InventoryId = i.InventoryId,
                InventoryName = i.InventoryName,
                InventoryTypeId = i.InventoryTypeId,
                NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                Quantity = i.Quantity,
                ServiceCodeId = i.ServiceCodeId,
                ServiceCodeName = sc.ServiceCode1
            };

            response.ContainerInventoryList.Add(new ContainerInventoryDTO(cDTO,iDTO,0));

            return response;
        }

        public GetContainerResponse GetContainers()
        {
            GetContainerResponse containers = new GetContainerResponse();

            List<ContainerType> types = dbContext.ContainerType.ToList();

            List<ContainerName> names = dbContext.ContainerName.ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.ToList();

            dbContext.Container.ToList().ForEach(item =>
            {
                ContainerDTO cDTO = new ContainerDTO()
                {
                    ContainerId = item.ContainerId,
                    ContainerTypeId = item.ContainerTypeId,
                    ContainerSize = item.ContainerSize,
                    ContainerName = names.Where(a => a.ContainerTypeId == item.ContainerTypeId).Select(b => b.Name).First(),
                    ContainerTypeName = types.Where(a => a.ContainerTypeId == item.ContainerTypeId).Select(b => b.ContainerTypeName).First()
                };

                long inventoryId = dbContext.InventoryContainerMap.Where(a => a.ContainerId == item.ContainerId).Select(b => b.InventoryId).First();

                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == inventoryId).First();

                ServiceCode sc = dbContext.ServiceCode.Where(a => a.ServiceCodeId == i.ServiceCodeId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity,
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = sc.ServiceCode1
                };

                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                containers.ContainerInventoryList.Add(new ContainerInventoryDTO(cDTO, iDTO,imageId));
            });

            return containers;
        }

        public CustomerContainerResponse GetCustomerContainers(CustomerContainerRequest request)
        {
            CustomerContainerResponse response = new CustomerContainerResponse();

            dbContext.CustomerContainer.Where(a => a.CustomerId == request.CustomerContainer.CustomerId).ToList().ForEach(item =>
            {
                response.CustomerContainers.Add(new CustomerContainerDTO()
                {
                    CustomerContainerId = item.CustomerContainerId,
                    CustomerId = item.CustomerId,
                    ImageId = item.ImageId,
                    Label = item.Label
                });
            });

            return response;
        }

        public ApiResponse AddUpdateCustomerContainer(CustomerContainerRequest request)
        {
            ApiResponse response = new ApiResponse();

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    CustomerContainer cc = new CustomerContainer();

                    if (request.CustomerContainer.CustomerContainerId == 0)
                    {
                        cc.CustomerId = request.CustomerContainer.CustomerId;
                        cc.Label = request.CustomerContainer.Label;
                        cc.ImageId = request.CustomerContainer.ImageId;

                        dbContext.CustomerContainer.Add(cc);
                    }
                    else
                    {
                        cc = dbContext.CustomerContainer.Where(a => a.CustomerContainerId == request.CustomerContainer.CustomerContainerId).FirstOrDefault();

                        if (cc != null && cc.CustomerContainerId == request.CustomerContainer.CustomerContainerId)
                        {
                            if (cc.ImageId != 0 && cc.ImageId != request.CustomerContainer.ImageId)
                            {
                                Image i = dbContext.Image.Where(a => a.ImageId == cc.ImageId).FirstOrDefault();

                                if (i != null && i.ImageId == cc.ImageId)
                                {
                                    dbContext.Image.Remove(i);
                                }
                            }

                            cc.Label = request.CustomerContainer.Label;
                            cc.ImageId = request.CustomerContainer.ImageId;
                        }
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    response.Id = cc.CustomerContainerId;
                }
            }
            catch (Exception ex)
            {
                int debug = 0;
            }

            return response;
        }

        public ApiResponse DeleteCustomerContainer(CustomerContainerRequest request)
        {
            ApiResponse response = new ApiResponse();

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    CustomerContainer cc = dbContext.CustomerContainer.Where(a => a.CustomerContainerId == request.CustomerContainer.CustomerContainerId).FirstOrDefault();
                    if (cc != null && cc.CustomerContainerId == request.CustomerContainer.CustomerContainerId)
                    {

                        if (cc.ImageId != 0)
                        {
                            Image i = dbContext.Image.Where(a => a.ImageId == request.CustomerContainer.ImageId).FirstOrDefault();
                            if (i != null && i.ImageId == request.CustomerContainer.ImageId)
                            {
                                dbContext.Image.Remove(i);
                            }
                        }

                        dbContext.CustomerContainer.Remove(cc);
                        dbContext.SaveChanges();
                        scope.Complete();
                        response.Id = cc.CustomerContainerId;
                    }
                }
            }
            catch (Exception ex)
            {
                int debug = 0;
            }

            return response;
        }

        public List<ServiceCodeDTO> GetServiceCodes()
        {
            List<ServiceCodeDTO> serviceCodes = new List<ServiceCodeDTO>();

            dbContext.ServiceCode.ToList().ForEach(item =>
            {
                serviceCodes.Add(new ServiceCodeDTO()
                {
                    ServiceCodeId = item.ServiceCodeId,
                    ServiceCode = item.ServiceCode1,
                    Description = item.Description,
                    Size = item.Size,
                    Price = item.Price,
                    Taxable = item.Taxable,
                    GeneralLedger = item.GeneralLedger
                });
            });

            return serviceCodes;
        }

        public ServiceCodeDTO GetServiceCodeById(long serviceCodeId)
        {
            ServiceCodeDTO dto = new ServiceCodeDTO();

            ServiceCode code = dbContext.ServiceCode.Where(a => a.ServiceCodeId == serviceCodeId).FirstOrDefault();

            if(code != null && code.ServiceCodeId > 0)
            {
                dto.Cost = code.Cost;
                dto.Description = code.Description;
                dto.GeneralLedger = code.GeneralLedger;
                dto.Price = code.Price;
                dto.ServiceCode = code.ServiceCode1;
                dto.ServiceCodeId = code.ServiceCodeId;
                dto.Size = code.Size;
                dto.Taxable = code.Taxable;
            }

            return dto;
        }

        public GetServiceCodeResponse GetServiceCodesByType(string serviceCodePrefix)
        {
            GetServiceCodeResponse response = new GetServiceCodeResponse();

            List<ServiceCodeDTO> serviceCodes = new List<ServiceCodeDTO>();

            //dbContext.ServiceCode.Where(a => a.ServiceCode1.StartsWith(serviceCodePrefix)).ToList().ForEach(item =>
            //{
            //    serviceCodes.Add(new ServiceCodeDTO()
            //    {
            //        ServiceCodeId = item.ServiceCodeId,
            //        ServiceCode = item.ServiceCode1,
            //        Description = item.Description,
            //        Size = item.Size,
            //        Price = item.Price,
            //        Taxable = item.Taxable,
            //        GeneralLedger = item.GeneralLedger
            //    });
            //});

            return response;
        }

        public GetKvpLongStringResponse GetInventoryList()
        {
            GetKvpLongStringResponse response = new GetKvpLongStringResponse();

            List<KeyValuePair<long, string>> inventoryList = new List<KeyValuePair<long, string>>();

            dbContext.Inventory.Select(a => new { a.InventoryId, a.InventoryName }).ToList().ForEach(item =>
             {
                 inventoryList.Add(new KeyValuePair<long, string>(item.InventoryId, item.InventoryName));
             });

            response.KvpList = inventoryList;

            return response;
        }

        public long ServiceCodeIsNotUnique(string serviceCode)
        {
            long serviceCodeId = 0;

            ServiceCode sc = dbContext.ServiceCode.Where(a => a.ServiceCode1 == serviceCode).FirstOrDefault();

            if (sc != null && sc.ServiceCodeId > 0)
            {
                serviceCodeId = sc.ServiceCodeId;
            }

            return serviceCodeId;
        }

        public bool GeneralLedgerIsNotUnique(string generalLedger)
        {
            bool notUnique = false;

            ServiceCode sc = dbContext.ServiceCode.Where(a => a.GeneralLedger == generalLedger).FirstOrDefault();

            if (sc != null && sc.ServiceCodeId > 0)
            {
                notUnique = true;
            }

            return notUnique;
        }

        public long InventoryExists(InventoryDTO inventoryDTO)
        {
            long inventoryId = 0;

            Inventory i = dbContext.Inventory.Where(a => a.InventoryName == inventoryDTO.InventoryName &&
                a.InventoryTypeId == inventoryDTO.InventoryTypeId).FirstOrDefault();

            if(i != null && i.InventoryId > 0)
            {
                inventoryId = i.InventoryId;
            }

            return inventoryId;
        }

        public bool InventoryNameIsNotUnique(string inventoryName)
        {
            bool notUnique = false;

            Inventory i = dbContext.Inventory.Where(a => a.InventoryName == inventoryName).FirstOrDefault();

            if (i != null && i.InventoryId > 0)
            {
                notUnique = true;
            }

            return notUnique;
        }

        

        public bool ContainerNameIsnotUnique(string containerName)
        {
            bool notUnique = false;

            Container c = dbContext.Container.Where(a => a.ContainerName == containerName).FirstOrDefault();

            if (c != null && c.ContainerId > 0)
            {
                notUnique = true;
            }

            return notUnique;
        }

        public bool ArrangementNameIsnotUnique(ArrangementDTO arrangement)
        {
            bool notUnique = false;

            Arrangement ar = dbContext.Arrangement.Where(a => a.ArrangementName == arrangement.ArrangementName).FirstOrDefault();

            if (ar != null && ar.ArrangementId > 0)
            {
                if (arrangement.ArrangementId != 0 && arrangement.ArrangementId != ar.ArrangementId)  //don't check self
                {
                    notUnique = true;
                }
            }

            return notUnique;
        }

        public long ServiceCodeExists(ServiceCodeDTO serviceCodeDTO)
        {
            long serviceCodeId = 0;

            ServiceCode svcCode = dbContext.ServiceCode
                .Where(a => a.Cost.HasValue && a.Cost == serviceCodeDTO.Cost &&
                a.Price == serviceCodeDTO.Price && a.Taxable == serviceCodeDTO.Taxable &&
                a.ServiceCode1 == serviceCodeDTO.ServiceCode).FirstOrDefault();

            if (svcCode != null && svcCode.ServiceCodeId > 0)
            {
                serviceCodeId = svcCode.ServiceCodeId;
            }

            return serviceCodeId;
        }

        public long AddServiceCode(ServiceCodeDTO serviceCodeDTO)
        {
            long serviceCodeId = 0;

            try
            {
                ServiceCode serviceCode = new ServiceCode();
                serviceCode.Description = serviceCodeDTO.Description;
                serviceCode.GeneralLedger = serviceCodeDTO.GeneralLedger;
                serviceCode.Price = serviceCodeDTO.Price;
                serviceCode.ServiceCode1 = serviceCodeDTO.ServiceCode;
                serviceCode.Size = serviceCodeDTO.Size;
                serviceCode.Taxable = serviceCodeDTO.Taxable;

                dbContext.ServiceCode.Add(serviceCode);
                dbContext.SaveChanges();
                serviceCodeId = serviceCode.ServiceCodeId;
            }
            catch (Exception ex)
            {
                int debug = 0;
            }
            return serviceCodeId;
        }

       

        public long AddPlantName(AddPlantNameRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    PlantName plantName = new PlantName()
                    {
                        Name = request.PlantName,
                        PlantTypeId = request.PlantTypeId
                    };

                    dbContext.PlantName.Add(plantName);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = plantName.PlantNameId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }
               
        public long AddPlantType(AddPlantTypeRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    PlantType plantType = new PlantType()
                    {
                        PlantTypeName = request.PlantTypeName,
                    };

                    dbContext.PlantType.Add(plantType);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = plantType.PlantTypeId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }

        public long PlantExists(PlantDTO plantDTO)
        {
            long plantId = 0;

            Plant p = dbContext.Plant.Where(a => a.PlantName == plantDTO.PlantName && a.PlantTypeId == plantDTO.PlantTypeId && a.PlantSize == plantDTO.PlantSize).FirstOrDefault();

            if (p != null && p.PlantId > 0)
            {
                plantId = p.PlantId;
            }

            return plantId;
        }

        public long DoesCustomerExist(AddCustomerRequest request)
        {
            long personId = 0;
           
            Person p = dbContext.Person.Where(a => a.FirstName == request.Customer.Person.first_name &&
                a.LastName == request.Customer.Person.last_name && a.Email == request.Customer.Person.email &&
                a.PhonePrimary == request.Customer.Person.phone_primary).FirstOrDefault();

            if (p != null)
                personId = p.PersonId;

            return personId;
        }

        public long PlantNameExists(string plantName)
        {
            long plantNameId = 0;

            PlantName plntName = dbContext.PlantName.Where(a => a.Name == plantName).FirstOrDefault();

            if (plntName != null && plntName.PlantNameId > 0)
            {
                plantNameId = plntName.PlantNameId;
            }

            return plantNameId;
        }

        public long PlantTypeExists(string typeName)
        {
            long plantTypeId = 0;

            PlantType plntType = dbContext.PlantType.Where(a => a.PlantTypeName == typeName).FirstOrDefault();

            if (plntType != null && plntType.PlantTypeId > 0)
            {
                plantTypeId = plntType.PlantTypeId;
            }

            return plantTypeId;
        }

        public bool PlantNameIsNotUnique(string plantName)
        {
            bool notUnique = false;

            Plant p = dbContext.Plant.Where(a => a.PlantName == plantName).FirstOrDefault();

            if (p != null && p.PlantId > 0)
            {
                notUnique = true;
            }

            return notUnique;
        }

        public long AddPlant(AddPlantRequest plantRequest)
        {
           long plant_id = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    Plant p = new Plant()
                    {
                        PlantName = plantRequest.Plant.PlantName,
                        PlantTypeId = plantRequest.Plant.PlantTypeId,
                        PlantNameId = plantRequest.Plant.PlantNameId,
                        PlantSize = plantRequest.Plant.PlantSize
                    };

                    dbContext.Plant.Add(p);

                    Inventory i = new Inventory()
                    {
                        InventoryName = plantRequest.Inventory.InventoryName,
                        InventoryTypeId = plantRequest.Inventory.InventoryTypeId,
                        ServiceCodeId = plantRequest.Inventory.ServiceCodeId
                    };

                    dbContext.Inventory.Add(i);

                    InventoryPlantMap invPlantMap = new InventoryPlantMap();
                    invPlantMap.InventoryId = i.InventoryId;
                    invPlantMap.PlantId = p.PlantId;

                    dbContext.InventoryPlantMap.Add(invPlantMap);

                    if (plantRequest.ImageId > 0)
                    {
                        InventoryImageMap iImgMap = new InventoryImageMap();
                        iImgMap.InventoryId = i.InventoryId;
                        iImgMap.ImageId = plantRequest.ImageId;

                        dbContext.InventoryImageMap.Add(iImgMap);
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    plant_id = p.PlantId;
                }
            }
            catch (Exception ex)
            {

            }

            return plant_id;
        }

        public long ImportPlant(ImportPlantRequest request)
        {
            long plantId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    long plantTypeId = request.AddPlantRequest.Plant.PlantTypeId;

                    if (!String.IsNullOrEmpty(request.PlantType))
                    {
                        //add plant type
                        if (plantTypeId == 0)
                        {
                            AddPlantTypeRequest plantTypeRequest = new AddPlantTypeRequest();
                            plantTypeRequest.PlantTypeName = request.PlantType;
                            plantTypeId = AddPlantType(plantTypeRequest);
                        }

                        request.AddPlantRequest.Plant.PlantTypeId = plantTypeId;
                    }

                    string plantTypeName = dbContext.PlantType.Where(a => a.PlantTypeId == plantTypeId).Select(b => b.PlantTypeName).First();

                   long plantNameId = request.AddPlantRequest.Plant.PlantNameId;

                    if (!String.IsNullOrEmpty(request.PlantName))
                    {
                        //add plant name
                        if (plantNameId == 0)
                        {
                            AddPlantNameRequest plantNameRequest = new AddPlantNameRequest();
                            plantNameRequest.PlantName = request.PlantName;
                            plantNameRequest.PlantTypeId = plantTypeId;
                            plantNameId = AddPlantName(plantNameRequest);
                        }

                        request.AddPlantRequest.Plant.PlantNameId = plantNameId;
                    }

                    string plantName = dbContext.PlantName.Where(a => a.PlantNameId == plantNameId).Select(b => b.Name).First();

                    long serviceCodeId = request.ServiceCode.ServiceCodeId;
                    string serviceCodeName = request.ServiceCode.ServiceCode;

                    if (serviceCodeId == 0)
                    {
                        serviceCodeId = AddServiceCode(request.ServiceCode);
                    }

                    ServiceCode svcCode = dbContext.ServiceCode.Where(a => a.ServiceCodeId == serviceCodeId).First();

                    serviceCodeName = svcCode.ServiceCode1;
                   

                    request.AddPlantRequest.Plant.PlantName = plantTypeName + "-" + plantName;
                    request.AddPlantRequest.Plant.PlantNameId = plantNameId;
                    request.AddPlantRequest.Plant.PlantTypeId = plantTypeId;
                    request.AddPlantRequest.Plant.PlantTypeName = plantTypeName;
                    request.AddPlantRequest.Plant.PlantSize = request.PlantSize;

                    request.AddPlantRequest.Inventory.InventoryTypeId = (long)Enums.InventoryType.Orchids;
                    request.AddPlantRequest.Inventory.InventoryTypeName = "Orchids";
                    request.AddPlantRequest.Inventory.InventoryName = plantTypeName + "-" + plantName;
                    request.AddPlantRequest.Inventory.ServiceCodeId = serviceCodeId;
                    request.AddPlantRequest.Inventory.ServiceCodeName = serviceCodeName;

                    plantId = PlantExists(request.AddPlantRequest.Plant);

                    long inventoryId = 0;
                    if(plantId > 0)
                    {
                        inventoryId = dbContext.InventoryPlantMap.Where(a => a.PlantId == plantId).Select(b => b.InventoryId).First();
                    }

                    long imageId = 0;

                    if (inventoryId > 0)
                    {
                        request.AddPlantRequest.Inventory.InventoryId = inventoryId;

                        InventoryPlantMap ipm = dbContext.InventoryPlantMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if(ipm != null && ipm.InventoryPlantMapId != 0)
                        {
                            request.AddPlantRequest.Plant.PlantId = ipm.PlantId;
                        }

                        InventoryImageMap iim = dbContext.InventoryImageMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if(iim != null && iim.InventoryImageMapId > 0)
                        {
                            if(iim.ImageId == 0)
                            {
                                if (request.imageBytes != null && request.imageBytes.Length > 0)
                                {
                                    //add image
                                    imageId = AddPlantImage(request.imageBytes);
                                }

                                request.AddPlantRequest.ImageId = imageId;
                            }
                        }
                    }
                    else
                    {
                        if (request.imageBytes != null && request.imageBytes.Length > 0)
                        {
                            //add image
                            imageId = AddPlantImage(request.imageBytes);
                        }

                        request.AddPlantRequest.ImageId = imageId;
                    }

                    if (plantId == 0)
                    {
                        plantId = AddPlant(request.AddPlantRequest);
                    }
                    else
                    {
                        UpdatePlant(request);
                    }

                    scope.Complete();
                }
            }
            catch(Exception ex)
            {
                int debug = 1;
            }

            return plantId;
        }

        /// <summary>
        /// At this point, not that much can be updated - inventory cost, retail, image - plant - size
        /// </summary>
        /// <param name="plantRequest"></param>
        /// <returns></returns>
        public long UpdatePlant(ImportPlantRequest plantRequest)
        {
           long plantId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    if(plantRequest.AddPlantRequest.Plant.PlantId != 0)
                   {
                        Plant p = dbContext.Plant.Where(a => a.PlantId == plantRequest.AddPlantRequest.Plant.PlantId).FirstOrDefault();

                        if(p != null && p.PlantId > 0)
                        {
                            p.PlantSize = plantRequest.AddPlantRequest.Plant.PlantSize;
                        }
                    }

                    if(plantRequest.AddPlantRequest.Inventory.InventoryId > 0)
                    {
                        ServiceCode code = dbContext.ServiceCode.Where(a => a.ServiceCodeId == plantRequest.AddPlantRequest.Inventory.ServiceCodeId).FirstOrDefault();

                        if(code != null && code.ServiceCodeId > 0)
                        {
                            code.Cost = plantRequest.ServiceCode.Cost;
                            code.Description = plantRequest.ServiceCode.Description;
                            code.GeneralLedger = plantRequest.ServiceCode.GeneralLedger;
                            code.Price = plantRequest.ServiceCode.Price;
                            code.Taxable = plantRequest.ServiceCode.Taxable;
                        }
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return plantId;
        }

        /// <summary>
        /// What if this plant is part of an arrangement?
        /// </summary>
        /// <param name="plantId"></param>
        /// <returns></returns>
        public bool DeletePlant(long plantId)
        {
            bool result = false;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    InventoryPlantMap ipm = dbContext.InventoryPlantMap.Where(a => a.PlantId == plantId).FirstOrDefault();

                    if (ipm != null)
                    {
                        ArrangementInventoryInventoryMap aiim = dbContext.ArrangementInventoryInventoryMap.Where(a => a.InventoryId == ipm.InventoryId).FirstOrDefault();

                        //What if this plant is part of an arrangement? - you can't delete it
                        if (aiim != null && aiim.ArrangementId == 0)
                        {
                            Plant p = dbContext.Plant.Where(a => a.PlantId == plantId).FirstOrDefault();

                            if (p != null && p.PlantId != 0)
                            {
                                dbContext.Remove(p);

                            }

                            long inventoryId = 0;
                            if (ipm != null && ipm.PlantId != 0)
                            {
                                inventoryId = ipm.InventoryId;
                                dbContext.Remove(ipm);
                            }

                            InventoryImageMap iim = dbContext.InventoryImageMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                            if (iim != null && iim.InventoryId != 0)
                            {
                                dbContext.Remove(iim);
                            }

                            WorkOrderInventoryMap woim = dbContext.WorkOrderInventoryMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                            if (woim != null && woim.WorkOrderInventoryMapId > 0)
                            {
                                dbContext.Remove(woim);
                            }

                            Inventory i = dbContext.Inventory.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                            if (i != null && i.InventoryId != 0)
                            {
                                dbContext.Remove(i);
                            }

                            dbContext.SaveChanges();
                            scope.Complete();
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return result;
        }

        /// <summary>
        /// What if this container is part of an arrangement?
        /// </summary>
        /// <param name="containerId"></param>
        /// <returns></returns>
        public bool DeleteContainer(long containerId)
        {
            bool result = false;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    Container c = dbContext.Container.Where(a => a.ContainerId == containerId).FirstOrDefault();

                    if (c != null && c.ContainerId != 0)
                    {
                        dbContext.Remove(c);

                    }

                    InventoryContainerMap icm = dbContext.InventoryContainerMap.Where(a => a.ContainerId == containerId).FirstOrDefault();

                   long inventoryId = 0;
                    if (icm != null && icm.ContainerId != 0)
                    {
                        inventoryId = icm.InventoryId;
                        dbContext.Remove(icm);
                    }

                    InventoryImageMap iim = dbContext.InventoryImageMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                    if (iim != null && iim.InventoryId != 0)
                    {
                        dbContext.Remove(iim);
                    }

                    Inventory i = dbContext.Inventory.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                    if (i != null && i.InventoryId != 0)
                    {
                        dbContext.Remove(i);
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        public long AddContainer(AddContainerRequest containerRequest)
        {
            bool success = false;
            long container_id = 0;

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    Container c = new Container()
                    {
                        ContainerName = containerRequest.Container.ContainerName,
                        ContainerTypeId = containerRequest.Container.ContainerTypeId,
                        ContainerTypeName = containerRequest.Container.ContainerTypeName,
                        ContainerSize = containerRequest.Container.ContainerSize
                    };

                    dbContext.Container.Add(c);

                    Inventory i = new Inventory()
                    {
                        InventoryName = containerRequest.Inventory.InventoryName,
                        InventoryTypeId = containerRequest.Inventory.InventoryTypeId,
                        ServiceCodeId = containerRequest.Inventory.ServiceCodeId
                    };

                    dbContext.Inventory.Add(i);

                    InventoryContainerMap invContainerMap = new InventoryContainerMap();
                    invContainerMap.InventoryId = i.InventoryId;
                    invContainerMap.ContainerId = c.ContainerId;

                    dbContext.InventoryContainerMap.Add(invContainerMap);

                    if (containerRequest.ImageId > 0)
                    {
                        ContainerImageMap cImgMap = new ContainerImageMap();
                        cImgMap.ContainerId = c.ContainerId;
                        cImgMap.ImageId = containerRequest.ImageId;

                        dbContext.ContainerImageMap.Add(cImgMap);
                    }

                    if (containerRequest.ImageId > 0)
                    {
                        InventoryImageMap iImgMap = new InventoryImageMap();
                        iImgMap.InventoryId = i.InventoryId;
                        iImgMap.ImageId = containerRequest.ImageId;

                        dbContext.InventoryImageMap.Add(iImgMap);
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    container_id = c.ContainerId;
                }
                catch (Exception ex)
                {
                    int debug = 1;
                }
            }

            return container_id;
        }

        public long UpdateArrangement(AddArrangementRequest arrangementRequest)
        {
            long arrangement_id = 0;
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    List<long> modifiedInventoryIds = arrangementRequest.ArrangementInventory.Select(a => a.InventoryId).ToList();

                    Arrangement arrangement = dbContext.Arrangement.Where(a => a.ArrangementId == arrangementRequest.Arrangement.ArrangementId).FirstOrDefault();

                    if (arrangement != null)
                    {
                        arrangement_id = arrangement.ArrangementId;

                        arrangement.ArrangementName = arrangementRequest.Arrangement.ArrangementName;
                        arrangement.DesignerName = arrangementRequest.Arrangement.DesignerName;
                        arrangement._180or360 = arrangementRequest.Arrangement._180or360;
                        arrangement.Container = arrangementRequest.Arrangement.Container;
                        arrangement.CustomerContainerId = arrangementRequest.Arrangement.CustomerContainerId;
                        arrangement.LocationName = arrangementRequest.Arrangement.LocationName;
                        arrangement.IsGift = arrangementRequest.Arrangement.IsGift;
                        arrangement.GiftMessage = arrangementRequest.Arrangement.GiftMessage;

                        List<long> deleteOriginalIds = new List<long>();

                        List<ArrangementInventoryInventoryMap> inventoryMapOriginal =
                            dbContext.ArrangementInventoryInventoryMap.Where(a => a.ArrangementId == arrangementRequest.Arrangement.ArrangementId).ToList();

                        List<long> requestInventoryIds = new List<long>();

                        foreach(ArrangementInventoryDTO x in arrangementRequest.ArrangementInventory)
                        {
                            if(x.ArrangementInventoryInventoryMapId == 0)
                            {
                                ArrangementInventoryInventoryMap aiim = new ArrangementInventoryInventoryMap()
                                {
                                    ArrangementId = arrangement_id,
                                    InventoryId = x.InventoryId,
                                    Quantity = x.Quantity
                                };

                                dbContext.ArrangementInventoryInventoryMap.Add(aiim);
                            }
                            else
                            {
                                requestInventoryIds.Add(x.ArrangementInventoryInventoryMapId);

                                ArrangementInventoryInventoryMap m =
                                    inventoryMapOriginal.Where(a => a.ArrangementInventoryInventoryMapId == x.ArrangementInventoryInventoryMapId).First();
                                m.Quantity = x.Quantity;
                            }
                        }

                        //deletion is possible - if there's anything in the request that isn't present in the "original" list
                        foreach(ArrangementInventoryInventoryMap aiim in inventoryMapOriginal)
                        {
                            if(!requestInventoryIds.Contains(aiim.ArrangementInventoryInventoryMapId))
                            {
                                dbContext.ArrangementInventoryInventoryMap.Remove(inventoryMapOriginal.Where(a => a.ArrangementInventoryInventoryMapId == aiim.ArrangementInventoryInventoryMapId).First());
                            }
                        }

                        List<NotInInventory> notInInventoryOld =
                            dbContext.NotInInventory.Where(a => a.ArrangementId == arrangement.ArrangementId).ToList();

                        dbContext.NotInInventory.RemoveRange(notInInventoryOld);

                        foreach(NotInInventoryDTO dto in arrangementRequest.NotInInventory)
                        {
                            NotInInventory nii = new NotInInventory()
                            {
                                ArrangementId = arrangement.ArrangementId,
                                NotInInventoryId = dto.NotInInventoryId,
                                NotInInventoryName = dto.NotInInventoryName,
                                NotInInventoryPrice = dto.NotInInventoryPrice,
                                NotInInventoryQuantity = dto.NotInInventoryQuantity,
                                NotInInventorySize = dto.NotInInventorySize
                            };

                            dbContext.NotInInventory.Add(nii);
                        }

                        dbContext.SaveChanges();
                        scope.Complete();
                        arrangement_id = arrangement.ArrangementId;
                    }
                }
                catch(Exception ex)
                {

                }
            }

            return arrangement_id;
        }
        public long AddArrangement(AddArrangementRequest arrangementRequest)
        {
            bool success = false;
            long arrangement_id = 0;

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    List<long> svcCodeIds = new List<long>();
                    foreach (ArrangementInventoryDTO aidto in arrangementRequest.ArrangementInventory)
                    {
                        svcCodeIds.Add(aidto.InventoryId);
                    }

                    //List<long> svcCodeIds = dbContext.Inventory.Where(b => arrangementRequest.ArrangementInventory.Contains(b.InventoryId)).Select(b => b.ServiceCodeId).ToList();

                    List<ServiceCode> svcCodes = dbContext.ServiceCode.Where(b => svcCodeIds.Contains(b.ServiceCodeId)).ToList();

                    ServiceCode newSvcCode = new ServiceCode();
                    newSvcCode.Taxable = true;
                    newSvcCode.Price = svcCodes.Sum(b => b.Price);
                    newSvcCode.Cost = svcCodes.Sum(b => b.Cost);
                    //check for duplication - or create service code name generator
                    newSvcCode.ServiceCode1 = arrangementRequest.Arrangement.ArrangementName;

                    dbContext.ServiceCode.Add(newSvcCode);
                    dbContext.SaveChanges();

                    Arrangement a = new Arrangement()
                    {
                        ArrangementName = arrangementRequest.Arrangement.ArrangementName,
                        DesignerName = arrangementRequest.Arrangement.DesignerName,
                        _180or360 = arrangementRequest.Arrangement._180or360,
                        Container = arrangementRequest.Arrangement.Container,
                        CustomerContainerId = arrangementRequest.Arrangement.CustomerContainerId,
                        LocationName= arrangementRequest.Arrangement.LocationName,
                        ServiceCodeId = newSvcCode.ServiceCodeId,
                        IsGift = arrangementRequest.Arrangement.IsGift,
                        GiftMessage = arrangementRequest.Arrangement.GiftMessage
                    };

                    dbContext.Arrangement.Add(a);

                    Inventory i = new Inventory()
                    {
                        InventoryName = arrangementRequest.Inventory.InventoryName,
                        InventoryTypeId = arrangementRequest.Inventory.InventoryTypeId,
                        ServiceCodeId = newSvcCode.ServiceCodeId
                    };

                    dbContext.Inventory.Add(i);
                    dbContext.SaveChanges();

                    ArrangementInventoryMap invArrangementMap = new ArrangementInventoryMap();
                    invArrangementMap.InventoryId = i.InventoryId;
                    invArrangementMap.ArrangementId = a.ArrangementId;

                    dbContext.ArrangementInventoryMap.Add(invArrangementMap);

                    if (arrangementRequest.ImageId > 0)
                    {
                        ArrangementImageMap aImgMap = new ArrangementImageMap();
                        aImgMap.ArrangmentId = a.ArrangementId;
                        aImgMap.ImageId = arrangementRequest.ImageId;

                        dbContext.ArrangementImageMap.Add(aImgMap);
                    }

                    foreach(ArrangementInventoryDTO dto in arrangementRequest.ArrangementInventory)
                    {
                        dbContext.ArrangementInventoryInventoryMap.Add(new ArrangementInventoryInventoryMap()
                        {
                            ArrangementId = a.ArrangementId,
                            InventoryId = dto.InventoryId,
                            Quantity = dto.Quantity
                        });
                    }

                    foreach(NotInInventoryDTO dto in arrangementRequest.NotInInventory)
                    {
                        dbContext.NotInInventory.Add(new NotInInventory() 
                        { 
                            ArrangementId = a.ArrangementId,
                            NotInInventoryId = dto.NotInInventoryId,
                            NotInInventoryName = dto.NotInInventoryName,
                            NotInInventoryPrice = dto.NotInInventoryPrice,
                            NotInInventoryQuantity = dto.NotInInventoryQuantity,
                            NotInInventorySize = dto.NotInInventorySize
                        });
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    arrangement_id = a.ArrangementId;
                }
                catch (Exception ex)
                {

                }
            }

            return arrangement_id;
        }

        public List<InventoryTypeDTO> GetInventoryTypes()
        {
            List<InventoryTypeDTO> iDTO = new List<InventoryTypeDTO>();

            dbContext.InventoryType.ToList().ForEach(item =>
            {
                iDTO.Add(new InventoryTypeDTO()
                {
                    InventoryTypeId = item.InventoryTypeId,
                    InventoryTypeName = item.InventoryTypeName
                });
            });
            return iDTO;
        }

        public GetInventoryResponse GetInventory(Enums.InventoryType inventoryType)
        {
            GetInventoryResponse response = new GetInventoryResponse();

            List<InventoryDTO> inventoryList = new List<InventoryDTO>();

            if(inventoryType == Enums.InventoryType.AllInventoryTypes)
            {
                dbContext.Inventory.ToList().ForEach(item => 
                {
                    inventoryList.Add(new InventoryDTO()
                    {
                        InventoryId = item.InventoryId,
                        InventoryName = item.InventoryName,
                        InventoryTypeId = item.InventoryTypeId,
                        NotifyWhenLowAmount = item.NotifyWhenLowAmount,
                        Quantity = item.Quantity,
                        ServiceCodeId = item.ServiceCodeId
                    });
                });
            }
            else
            {
               long invType = (long)inventoryType;
                dbContext.Inventory.Where(a => a.InventoryTypeId == invType).ToList().ForEach(item =>
                {
                    inventoryList.Add(new InventoryDTO()
                    {
                        InventoryId = item.InventoryId,
                        InventoryName = item.InventoryName,
                        InventoryTypeId = item.InventoryTypeId,
                        NotifyWhenLowAmount = item.NotifyWhenLowAmount,
                        Quantity = item.Quantity,
                        ServiceCodeId = item.ServiceCodeId
                    });
                });
            }

            response.InventoryList = inventoryList;

            return response;
        }

        public WorkOrderResponse GetWorkOrder(long workOrderId)
        {
            WorkOrderResponse workOrderResponse = new WorkOrderResponse();

            WorkOrder wo = dbContext.WorkOrder.Where(a => a.WorkOrderId == workOrderId).FirstOrDefault();

            if (wo != null)
            {
                WorkOrderDTO w = new WorkOrderDTO()
                {
                    Buyer = wo.PersonReceiver,
                    Seller = wo.PersonInitiator,
                    DeliveredBy = wo.PersonDelivery,
                    DeliverTo = wo.DeliveryReceiver,
                    ClosedDate = wo.ClosedDate.HasValue ? wo.ClosedDate.Value : DateTime.MinValue,
                    Comments = wo.Comments,
                    CreateDate = wo.CreateDate.HasValue ? wo.CreateDate.Value : DateTime.MinValue,
                    DeliveryDate = wo.DeliveryDate.HasValue ? wo.DeliveryDate.Value : DateTime.MinValue,
                    Paid = wo.Paid,
                    IsCancelled = wo.IsCancelled,
                    UpdateDate = wo.UpdateDate.HasValue ? wo.UpdateDate.Value : DateTime.MinValue,
                    WorkOrderId = wo.WorkOrderId,
                    OrderType = wo.OrderType,
                    IsDelivery = wo.IsDelivery,
                    CustomerId = wo.CustomerId,
                    SellerId = wo.SellerId,
                    DeliveryType = wo.DeliveryType,
                    DeliveryUserId = wo.DeliveryUserId,
                    DeliveryRecipientId = wo.DeliveryRecipientId,
                    Delivered = wo.Delivered
                };

                List<WorkOrderInventoryMapDTO> inventoryList = new List<WorkOrderInventoryMapDTO>();

                dbContext.WorkOrderInventoryMap.Where(a => a.WorkOrderId == wo.WorkOrderId).ToList().ForEach(map =>
                {
                    {
                        inventoryList.Add(new WorkOrderInventoryMapDTO()
                        {
                            WorkOrderInventoryMapId = map.WorkOrderInventoryMapId,
                            WorkOrderId = map.WorkOrderId,
                            InventoryId = map.InventoryId,
                            InventoryName = dbContext.Inventory.Where(b => b.InventoryId == map.InventoryId).Select(c => c.InventoryName).First(),
                            Quantity = map.Quantity,
                            GroupId = map.GroupId,
                        });
                    }
                });

                List<long> inventoryIds = inventoryList.Select(a => a.InventoryId).ToList();

                dbContext.Inventory.Where(a => inventoryIds.Contains(a.InventoryId)).ToList().ForEach(item =>
                {
                    var i = inventoryList.Where(a => a.InventoryId == item.InventoryId).First();
                    i.InventoryName = item.InventoryName;
                });

                List<long> arrangementIds = dbContext.WorkOrderArrangementMap
                    .Where(a => a.WorkOrderId == workOrderId).Select(b => b.ArrangementId).ToList();

                foreach (long arrangementId in arrangementIds)
                {
                    workOrderResponse.Arrangements.Add(GetArrangement(arrangementId));
                }

                List<NotInInventoryDTO> notInInventory = new List<NotInInventoryDTO>();

                dbContext.NotInInventory.Where(a => a.WorkOrderId == workOrderId).ToList().ForEach(item =>
                {
                    if (item.ArrangementId == null || item.ArrangementId == 0)
                    {
                        notInInventory.Add(new NotInInventoryDTO()
                        {
                            NotInInventoryId = item.NotInInventoryId,
                            WorkOrderId = item.WorkOrderId,
                            ArrangementId = item.ArrangementId,
                            NotInInventoryName = item.NotInInventoryName,
                            NotInInventorySize = item.NotInInventorySize,
                            NotInInventoryQuantity = item.NotInInventoryQuantity,
                            NotInInventoryPrice = item.NotInInventoryPrice
                        });
                    }
                });

                List<WorkOrderImageMapDTO> imageMap = new List<WorkOrderImageMapDTO>();

                dbContext.WorkOrderImageMap.Where(a => a.WorkOrderId == workOrderId).ToList().ForEach(item =>
                {
                    Image i = dbContext.Image.Where(b => b.ImageId == item.ImageId).First();

                    imageMap.Add(new WorkOrderImageMapDTO()
                    {
                        WorkOrderImageMapId = item.WorkOrderImageMapId,
                        WorkOrderId = item.WorkOrderId,
                        ImageId = item.ImageId,
                        ImageData = i.ImageData
                    });
                });

                workOrderResponse.WorkOrder = w;
                workOrderResponse.ImageMap = imageMap;
                workOrderResponse.WorkOrderList = inventoryList;
                workOrderResponse.NotInInventory = notInInventory;
            }
            
            return workOrderResponse;
        }

        public List<WorkOrderResponse> GetWorkOrders(DateTime afterDate)
        {
            List<WorkOrderResponse> response = new List<WorkOrderResponse>();

            return response;
        }

        public List<WorkOrderResponse> GetWorkOrders(WorkOrderListFilter filter)
        {
            List<WorkOrderResponse> workOrderList = new List<WorkOrderResponse>();

            //get them all - won't do any work until you execute ToList()
            var query = dbContext.WorkOrder.Where(a => a.WorkOrderId > 0);

            if (filter.FromDate != null && filter.ToDate != null && filter.FromDate > DateTime.MinValue && filter.ToDate > DateTime.MinValue)
            {
                query = query.Where(a => a.CreateDate.HasValue && a.CreateDate.Value >= filter.FromDate && a.CreateDate.Value <= filter.ToDate);
            } 
            
            if(filter.CustomerId.HasValue)
            {
                query = query.Where(a => a.CustomerId == filter.CustomerId.Value);
            }

            if(filter.Paid.HasValue)
            {
                query = query.Where(a => a.Paid == filter.Paid.Value);
            }

            if (filter.Delivery.HasValue)
            {
                query = query.Where(a => a.IsDelivery == filter.Delivery.Value);
            }

            if(filter.DeliveryDate.HasValue && filter.DeliveryDate > DateTime.MinValue)
            {
                query = query.Where(a => a.DeliveryDate == filter.DeliveryDate.Value);
            }

            if (filter.SiteService.HasValue)
            {
                query = query.Where(a => a.IsSiteService == filter.SiteService.Value);
            }

            if(filter.DeliveryUserId.HasValue)
            {
                query = query.Where(a => a.DeliveryUserId == filter.DeliveryUserId);
            }

            query.ToList().ForEach(wo =>
            {
                WorkOrderDTO w = new WorkOrderDTO()
                {
                    Buyer = wo.PersonReceiver,
                    Seller = wo.PersonInitiator,
                    DeliveredBy = wo.PersonDelivery,
                    DeliverTo = wo.DeliveryReceiver,
                    ClosedDate = wo.ClosedDate.HasValue ? wo.ClosedDate.Value : DateTime.MinValue,
                    Comments = wo.Comments,
                    CreateDate = wo.CreateDate.HasValue ? wo.CreateDate.Value : DateTime.MinValue,
                    DeliveryDate = wo.DeliveryDate.HasValue ? wo.DeliveryDate.Value : DateTime.MinValue,
                    Paid = wo.Paid,
                    UpdateDate = wo.UpdateDate.HasValue ? wo.UpdateDate.Value : DateTime.MinValue,
                    WorkOrderId = wo.WorkOrderId,
                    CustomerId = wo.CustomerId,
                    OrderType = wo.OrderType,
                    IsDelivery = wo.IsDelivery,
                    IsCancelled = wo.IsCancelled,
                    IsSiteService = wo.IsSiteService,
                    DeliveryType = wo.DeliveryType,
                    DeliveryUserId = wo.DeliveryUserId,
                    DeliveryRecipientId = wo.DeliveryRecipientId,
                    Delivered = wo.Delivered
                };

                List<WorkOrderInventoryMapDTO> inventoryList = new List<WorkOrderInventoryMapDTO>();
                    
                dbContext.WorkOrderInventoryMap.Where(a => a.WorkOrderId == wo.WorkOrderId).ToList().ForEach(map =>
                {
                    { 
                        inventoryList.Add(new WorkOrderInventoryMapDTO()
                        {
                            WorkOrderInventoryMapId = map.WorkOrderInventoryMapId,
                            WorkOrderId = map.WorkOrderId,
                            InventoryId = map.InventoryId,
                            InventoryName = dbContext.Inventory.Where(b => b.InventoryId == map.InventoryId).Select(c => c.InventoryName).First(),
                            Quantity = map.Quantity,
                            GroupId = map.GroupId,
                        });
                    }
                });

                List<long> inventoryIds = inventoryList.Select(a => a.InventoryId).ToList();

                dbContext.Inventory.Where(a => inventoryIds.Contains(a.InventoryId)).ToList().ForEach(item =>
                {
                    var i = inventoryList.Where(a => a.InventoryId == item.InventoryId).First();
                    i.InventoryName = item.InventoryName;
                });

                List<NotInInventoryDTO> notInInventory = new List<NotInInventoryDTO>();

                dbContext.NotInInventory.Where(a => a.WorkOrderId == wo.WorkOrderId).ToList().ForEach(item =>
                {
                    notInInventory.Add(new NotInInventoryDTO()
                    {
                        NotInInventoryId = item.NotInInventoryId,
                        WorkOrderId = item.WorkOrderId,
                        ArrangementId = item.ArrangementId,
                        NotInInventoryName = item.NotInInventoryName,
                        NotInInventorySize = item.NotInInventorySize,
                        NotInInventoryQuantity = item.NotInInventoryQuantity,
                        NotInInventoryPrice = item.NotInInventoryPrice
                    });
                });

                List<GetArrangementResponse> arrangements = new List<GetArrangementResponse>();

                dbContext.WorkOrderArrangementMap.Where(a => a.WorkOrderId == w.WorkOrderId).ToList().ForEach(arrangement =>
                {
                    arrangements.Add(GetArrangement(arrangement.ArrangementId));
                });
                
                List<WorkOrderImageMapDTO> imageMap = new List<WorkOrderImageMapDTO>();

                dbContext.WorkOrderImageMap.Where(a => a.WorkOrderId == wo.WorkOrderId).ToList().ForEach(item =>
                {
                    imageMap.Add(new WorkOrderImageMapDTO()
                    {
                        WorkOrderImageMapId = item.WorkOrderImageMapId,
                        WorkOrderId = item.WorkOrderId,
                        ImageId = item.ImageId
                    }); 
                });

                WorkOrderResponse r = new WorkOrderResponse();
                r.WorkOrder = w;
                r.Arrangements = arrangements;
                r.WorkOrderList = inventoryList;
                r.NotInInventory = notInInventory;
                r.ImageMap = imageMap;

                workOrderList.Add(r);
            });
  
            
            return workOrderList;
        }

        public GetPlantResponse GetPlant(long plantId)
        {
            GetPlantResponse response = new GetPlantResponse();

            Plant p = dbContext.Plant.Where(b => b.PlantId == plantId).FirstOrDefault();

            if(p != null && p.PlantId != 0)
            {
                long inventoryId = dbContext.InventoryPlantMap.Where(a => a.PlantId == p.PlantId).Select(b => b.InventoryId).First();

                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                if (i != null)
                {
                    long imageId = dbContext.InventoryImageMap.Where(a => a.InventoryId == inventoryId).Select(b => b.ImageId).FirstOrDefault();

                    PlantType plantType = dbContext.PlantType.Where(a => a.PlantTypeId == p.PlantTypeId).First();

                    InventoryType inventoryType = dbContext.InventoryType.Where(a => a.InventoryTypeId == i.InventoryTypeId).First();

                    ServiceCode serviceCode = dbContext.ServiceCode.Where(a => a.ServiceCodeId == i.ServiceCodeId).First();

                    PlantDTO pDTO = new PlantDTO();
                    pDTO.PlantId = p.PlantId;
                    pDTO.PlantName = p.PlantName;
                    pDTO.PlantTypeId = p.PlantTypeId;
                    pDTO.PlantTypeName = plantType.PlantTypeName;

                    InventoryDTO iDTO = new InventoryDTO();
                    iDTO.InventoryId = i.InventoryId;
                    iDTO.InventoryName = i.InventoryName;
                    iDTO.InventoryTypeId = i.InventoryTypeId;
                    iDTO.InventoryTypeName = inventoryType.InventoryTypeName;
                    iDTO.NotifyWhenLowAmount = i.NotifyWhenLowAmount;
                    iDTO.Quantity = i.Quantity;
                    iDTO.ServiceCodeId = i.ServiceCodeId;
                    iDTO.ServiceCodeName = serviceCode.ServiceCode1;

                    response.PlantInventoryList.Add(new PlantInventoryDTO(pDTO, iDTO, imageId));
                }
            }

            return response;
        }

        public GetPlantTypeResponse GetPlantTypes()
        {
            GetPlantTypeResponse response = new GetPlantTypeResponse(new List<PlantTypeDTO>());

            dbContext.PlantType.ToList().ForEach(item =>
            {
                response.PlantTypes.Add(new PlantTypeDTO()
                {
                    PlantTypeId = item.PlantTypeId,
                    PlantTypeName = item.PlantTypeName
                });
            });

            return response;
        }

        public GetPlantNameResponse GetPlantNamesByType(long plantTypeId)
        {
            GetPlantNameResponse response = new GetPlantNameResponse();

            List<PlantNameDTO> plantNames = new List<PlantNameDTO>();

            dbContext.PlantName.Where(a => a.PlantTypeId == plantTypeId).ToList().ForEach(item =>
            {
                plantNames.Add(new PlantNameDTO()
                {
                    PlantNameId = item.PlantNameId,
                    PlantTypeId = item.PlantTypeId,
                    PlantName = item.Name
                });
            });

            response.PlantNames = plantNames;

            return response;
        }
        public GetPlantResponse GetPlantsByType(long plantTypeId)
        {
            GetPlantResponse response = new GetPlantResponse();

            List<long> plantIds = dbContext.Plant.Where(b => b.PlantTypeId == plantTypeId).Select(a => a.PlantId).ToList();

            List<InventoryPlantMap> inventoryPlantMap = dbContext.InventoryPlantMap.Where(a => plantIds.Contains(a.PlantId)).ToList();

            List<long> inventoryIds = inventoryPlantMap.Select(a => a.InventoryId).ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.Where(a => inventoryIds.Contains(a.InventoryId)).ToList();

            GetPlantTypeResponse plantTypeList = GetPlantTypes();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            foreach (InventoryPlantMap map in inventoryPlantMap)
            {
                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == map.InventoryId).First();

                Plant p = dbContext.Plant.Where(b => b.PlantId == map.PlantId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    InventoryTypeName = "Plants", // inventoryTypeList.Where(a => a.InventoryTypeId == i.InventoryTypeId).Select(b => b.InventoryTypeName).First(),
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = serviceCodeList.Where(a => a.ServiceCodeId == i.ServiceCodeId).Select(b => b.ServiceCode).First(),
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity
                };

                PlantDTO pDTO = new PlantDTO()
                {
                    PlantId = p.PlantId,
                    PlantName = p.PlantName,
                    PlantTypeId = p.PlantTypeId,
                    PlantSize = p.PlantSize,
                    PlantTypeName = plantTypeList.PlantTypes.Where(a => a.PlantTypeId == p.PlantTypeId).Select(b => b.PlantTypeName).First()
                };

                //plantResponse.Plant = pDTO;
                //plantResponse.Inventory = iDTO;
                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                response.PlantInventoryList.Add(new PlantInventoryDTO(pDTO,iDTO,imageId));
            }
            return response;
        }

        public GetPlantResponse GetPlants()
        {
            GetPlantResponse response = new GetPlantResponse();

            GetPlantTypeResponse plantTypeList = GetPlantTypes();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            List<InventoryType> inventoryTypeList = dbContext.InventoryType.ToList();
            
            List<InventoryPlantMap> inventoryPlantMap = dbContext.InventoryPlantMap.ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.ToList();

            foreach(InventoryPlantMap map in inventoryPlantMap)
            {
                //GetPlantResponse plantResponse = new GetPlantResponse();

                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == map.InventoryId).First();

                Plant p = dbContext.Plant.Where(b => b.PlantId == map.PlantId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    InventoryTypeName = inventoryTypeList.Where(a => a.InventoryTypeId == i.InventoryTypeId).Select(b => b.InventoryTypeName).First(),
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = serviceCodeList.Where(a => a.ServiceCodeId == i.ServiceCodeId).Select(b => b.ServiceCode).First(),
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity
                };

                PlantDTO pDTO = new PlantDTO()
                {
                    PlantId = p.PlantId,
                    PlantName = p.PlantName,
                    PlantTypeId = p.PlantTypeId,
                    PlantSize = p.PlantSize,
                    PlantTypeName = plantTypeList.PlantTypes.Where(a => a.PlantTypeId == p.PlantTypeId).Select(b => b.PlantTypeName).First()
                };

                //plantResponse.Plant = pDTO;
                //plantResponse.Inventory = iDTO;
                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                response.PlantInventoryList.Add(new PlantInventoryDTO(pDTO,iDTO,imageId));
            }

            return response;
        }

        public GetMaterialTypeResponse GetMaterialTypes()
        {
            GetMaterialTypeResponse response = new GetMaterialTypeResponse(new List<MaterialTypeDTO>());

            dbContext.MaterialType.ToList().ForEach(item =>
            {
                response.MaterialTypes.Add(new MaterialTypeDTO()
                {
                    MaterialTypeId = item.MaterialTypeId,
                    MaterialTypeName = item.MaterialTypeName
                });
            });

            return response;
        }

        public GetMaterialNameResponse GetMaterialNamesByType(long materialTypeId)
        {
            GetMaterialNameResponse response = new GetMaterialNameResponse();

            List<MaterialNameDTO> materialNames = new List<MaterialNameDTO>();

            dbContext.MaterialName.Where(a => a.MaterialTypeId == materialTypeId).ToList().ForEach(item =>
            {
                materialNames.Add(new MaterialNameDTO()
                {
                    MaterialNameId = item.MaterialNameId,
                    MaterialTypeId = item.MaterialTypeId,
                    MaterialName = item.Name
                });
            });

            response.MaterialNames = materialNames;

            return response;
        }
        public GetMaterialResponse GetMaterialsByType(long materialTypeId)
        {
            GetMaterialResponse response = new GetMaterialResponse();

            List<long> materialIds = dbContext.Material.Where(b => b.MaterialTypeId == materialTypeId).Select(a => a.MaterialId).ToList();

            List<InventoryMaterialMap> inventoryMaterialMap = dbContext.InventoryMaterialMap.Where(a => materialIds.Contains(a.MaterialId)).ToList();

            List<long> inventoryIds = inventoryMaterialMap.Select(a => a.InventoryId).ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.Where(a => inventoryIds.Contains(a.InventoryId)).ToList();

            GetMaterialTypeResponse materialTypeList = GetMaterialTypes();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            foreach (InventoryMaterialMap map in inventoryMaterialMap)
            {
                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == map.InventoryId).First();

                Material m = dbContext.Material.Where(b => b.MaterialId == map.MaterialId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    InventoryTypeName = "Plants", // inventoryTypeList.Where(a => a.InventoryTypeId == i.InventoryTypeId).Select(b => b.InventoryTypeName).First(),
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = serviceCodeList.Where(a => a.ServiceCodeId == i.ServiceCodeId).Select(b => b.ServiceCode).First(),
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity
                };

                MaterialDTO mDTO = new MaterialDTO()
                {
                    MaterialId = m.MaterialId,
                    MaterialName = m.MaterialName,
                    MaterialTypeId = m.MaterialTypeId,
                    MaterialTypeName = materialTypeList.MaterialTypes.Where(a => a.MaterialTypeId == m.MaterialTypeId).Select(b => b.MaterialTypeName).First()
                };

                //plantResponse.Plant = pDTO;
                //plantResponse.Inventory = iDTO;
                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                response.MaterialInventoryList.Add(new MaterialInventoryDTO(mDTO, iDTO, imageId));
            }
            return response;
        }
        public GetMaterialResponse GetMaterials()
        {
            GetMaterialResponse response = new GetMaterialResponse();

            GetMaterialTypeResponse materialTypeList = GetMaterialTypes();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            List<InventoryType> inventoryTypeList = dbContext.InventoryType.ToList();

            List<InventoryMaterialMap> inventoryMaterialMap = dbContext.InventoryMaterialMap.ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.ToList();

            foreach (InventoryMaterialMap map in inventoryMaterialMap)
            {
                //GetPlantResponse plantResponse = new GetPlantResponse();

                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == map.InventoryId).First();

                Material m = dbContext.Material.Where(b => b.MaterialId == map.MaterialId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    InventoryTypeName = inventoryTypeList.Where(a => a.InventoryTypeId == i.InventoryTypeId).Select(b => b.InventoryTypeName).First(),
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = serviceCodeList.Where(a => a.ServiceCodeId == i.ServiceCodeId).Select(b => b.ServiceCode).First(),
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity
                };

                MaterialDTO mDTO = new MaterialDTO()
                {
                    MaterialId = m.MaterialId,
                    MaterialName = m.MaterialName,
                    MaterialSize = m.MaterialSize,
                    MaterialTypeId = m.MaterialTypeId,
                    MaterialTypeName = materialTypeList.MaterialTypes.Where(a => a.MaterialTypeId == m.MaterialTypeId).Select(b => b.MaterialTypeName).First()
                };

                //plantResponse.Plant = pDTO;
                //plantResponse.Inventory = iDTO;
                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                response.MaterialInventoryList.Add(new MaterialInventoryDTO(mDTO, iDTO, imageId));
            }

            return response;
        }

        public GetFoliageTypeResponse GetFoliageTypes()
        {
            GetFoliageTypeResponse response = new GetFoliageTypeResponse(new List<FoliageTypeDTO>());

            dbContext.FoliageType.ToList().ForEach(item =>
            {
                response.FoliageTypes.Add(new FoliageTypeDTO()
                {
                    FoliageTypeId = item.FoliageTypeId,
                    FoliageTypeName = item.FoliageTypeName
                });
            });

            return response;
        }

        public GetFoliageNameResponse GetFoliageNamesByType(long foliageTypeId)
        {
            GetFoliageNameResponse response = new GetFoliageNameResponse();

            List<FoliageNameDTO> foliageNames = new List<FoliageNameDTO>();

            dbContext.FoliageName.Where(a => a.FoliageTypeId == foliageTypeId).ToList().ForEach(item =>
            {
                foliageNames.Add(new FoliageNameDTO()
                {
                    FoliageNameId = item.FoliageNameId,
                    FoliageTypeId = item.FoliageTypeId,
                    FoliageName = item.Name
                });
            });

            response.FoliageNames = foliageNames;

            return response;
        }
        public GetFoliageResponse GetFoliageByType(long foliageTypeId)
        {
            GetFoliageResponse response = new GetFoliageResponse();

            List<long> foliageIds = dbContext.Foliage.Where(b => b.FoliageTypeId == foliageTypeId).Select(a => a.FoliageId).ToList();

            List<InventoryFoliageMap> inventoryFoliageMap = dbContext.InventoryFoliageMap.Where(a => foliageIds.Contains(a.FoliageId)).ToList();

            List<long> inventoryIds = inventoryFoliageMap.Select(a => a.InventoryId).ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.Where(a => inventoryIds.Contains(a.InventoryId)).ToList();

            GetFoliageTypeResponse foliageTypeList = GetFoliageTypes();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            foreach (InventoryFoliageMap map in inventoryFoliageMap)
            {
                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == map.InventoryId).First();

                Foliage f = dbContext.Foliage.Where(b => b.FoliageId == map.FoliageId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    InventoryTypeName = "Foliage", // inventoryTypeList.Where(a => a.InventoryTypeId == i.InventoryTypeId).Select(b => b.InventoryTypeName).First(),
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = serviceCodeList.Where(a => a.ServiceCodeId == i.ServiceCodeId).Select(b => b.ServiceCode).First(),
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity
                };

                FoliageDTO fDTO = new FoliageDTO()
                {
                    FoliageId = f.FoliageId,
                    FoliageName = f.FoliageName,
                    FoliageSize = f.FoliageSize,
                    FoliageTypeId = f.FoliageTypeId,
                    FoliageTypeName = foliageTypeList.FoliageTypes.Where(a => a.FoliageTypeId == f.FoliageTypeId).Select(b => b.FoliageTypeName).First()
                };

                //plantResponse.Plant = pDTO;
                //plantResponse.Inventory = iDTO;
                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                response.FoliageInventoryList.Add(new FoliageInventoryDTO(fDTO, iDTO, imageId));
            }
            return response;
        }
        public GetFoliageResponse GetFoliage()
        {
            GetFoliageResponse response = new GetFoliageResponse();

            GetFoliageTypeResponse foliageTypeList = GetFoliageTypes();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            List<InventoryType> inventoryTypeList = dbContext.InventoryType.ToList();

            List<InventoryFoliageMap> inventoryFoliageMap = dbContext.InventoryFoliageMap.ToList();

            List<InventoryImageMap> inventoryImageMap = dbContext.InventoryImageMap.ToList();

            foreach (InventoryFoliageMap map in inventoryFoliageMap)
            {
                //GetPlantResponse plantResponse = new GetPlantResponse();

                Inventory i = dbContext.Inventory.Where(a => a.InventoryId == map.InventoryId).First();

                Foliage f = dbContext.Foliage.Where(b => b.FoliageId == map.FoliageId).First();

                InventoryDTO iDTO = new InventoryDTO()
                {
                    InventoryId = i.InventoryId,
                    InventoryName = i.InventoryName,
                    InventoryTypeId = i.InventoryTypeId,
                    InventoryTypeName = inventoryTypeList.Where(a => a.InventoryTypeId == i.InventoryTypeId).Select(b => b.InventoryTypeName).First(),
                    ServiceCodeId = i.ServiceCodeId,
                    ServiceCodeName = serviceCodeList.Where(a => a.ServiceCodeId == i.ServiceCodeId).Select(b => b.ServiceCode).First(),
                    NotifyWhenLowAmount = i.NotifyWhenLowAmount,
                    Quantity = i.Quantity
                };

                FoliageDTO fDTO = new FoliageDTO()
                {
                    FoliageId = f.FoliageId,
                    FoliageName = f.FoliageName,
                    FoliageSize = f.FoliageSize,
                    FoliageTypeId = f.FoliageTypeId,
                    FoliageTypeName = foliageTypeList.FoliageTypes.Where(a => a.FoliageTypeId == f.FoliageTypeId).Select(b => b.FoliageTypeName).First()
                };

                //plantResponse.Plant = pDTO;
                //plantResponse.Inventory = iDTO;
                long imageId = inventoryImageMap.Where(a => a.InventoryId == i.InventoryId).Select(b => b.ImageId).FirstOrDefault();

                response.FoliageInventoryList.Add(new FoliageInventoryDTO(fDTO, iDTO, imageId));
            }

            return response;
        }

        public List<GetSimpleArrangementResponse> GetArrangements(string arrangementName)
        {
            List<GetSimpleArrangementResponse> responseList = new List<GetSimpleArrangementResponse>();


            dbContext.Arrangement.Where(a => a.ArrangementName.Contains(arrangementName)).ToList().ForEach(item => 
            {
                GetSimpleArrangementResponse r = new GetSimpleArrangementResponse()
                {
                    Arrangement = new ArrangementDTO()
                    {
                        ArrangementId = item.ArrangementId,
                        ArrangementName = item.ArrangementName,
                        DesignerName = item.DesignerName,
                        _180or360 = item._180or360,
                        Container = item.Container,
                        CustomerContainerId = item.CustomerContainerId,
                        LocationName = item.LocationName,
                        ServiceCodeId = item.ServiceCodeId,
                        UpdateDate = item.UpdateDate,
                        IsGift = item.IsGift,
                        GiftMessage = item.GiftMessage
                    }
                };

                long inventoryId = dbContext.ArrangementInventoryMap.Where(b => b.ArrangementId == item.ArrangementId).Select(b => b.InventoryId).FirstOrDefault();

                Inventory i = dbContext.Inventory.Where(c => c.InventoryId == inventoryId).FirstOrDefault();

                if (i != null)
                {
                    InventoryDTO inventory = new InventoryDTO()
                    {
                        InventoryId = i.InventoryId,
                        InventoryName = i.InventoryName,
                        InventoryTypeId = i.InventoryTypeId,
                    };

                    r.Inventory = inventory;

                    responseList.Add(r);
                }
            });

            return responseList;
        }

        public GetArrangementResponse GetArrangement(long arrangementId)
        {
            GetArrangementResponse response = new GetArrangementResponse();

            List<ServiceCodeDTO> serviceCodeList = GetServiceCodes();

            Arrangement a = dbContext.Arrangement.Where(b => b.ArrangementId == arrangementId).First();

            if (a != null)
            {
                ArrangementImageMap arrangementImageMap = dbContext.ArrangementImageMap.Where(b => b.ArrangmentId == a.ArrangementId).FirstOrDefault();

                ArrangementInventoryMap arrangementInventoryMap = dbContext.ArrangementInventoryMap.Where(b => b.ArrangementId == a.ArrangementId).First();

                List<ArrangementInventoryInventoryMap> aiim = dbContext.ArrangementInventoryInventoryMap.Where(b => b.ArrangementId == a.ArrangementId).ToList();

                Inventory i = dbContext.Inventory.Where(b => b.InventoryId == arrangementInventoryMap.InventoryId).First();

                List<long> inventoryIds = dbContext.ArrangementInventoryInventoryMap.Where(b => b.ArrangementId == a.ArrangementId).Select(c => c.InventoryId).ToList();

                List<Inventory> inventoryList = dbContext.Inventory.Where(b => inventoryIds.Contains(b.InventoryId)).ToList();

                List<InventoryImageMap> iim = dbContext.InventoryImageMap.Where(b => inventoryIds.Contains(b.InventoryId)).ToList();

                response.Arrangement = new ArrangementDTO()
                {
                    ArrangementId = a.ArrangementId,
                    ArrangementName = a.ArrangementName,
                    DesignerName = a.DesignerName,
                    _180or360 = a._180or360,
                    Container = a.Container,
                    CustomerContainerId = a.CustomerContainerId,
                    LocationName = a.LocationName,
                    ServiceCodeId = serviceCodeList.Where(b => b.ServiceCodeId == a.ServiceCodeId).Select(c => c.ServiceCodeId).First(),
                    IsGift = a.IsGift,
                    GiftMessage = a.GiftMessage
                };

                response.Inventory.InventoryId = i.InventoryId;
                response.Inventory.InventoryName = i.InventoryName;
                response.Inventory.InventoryTypeId = i.InventoryTypeId;
                response.Inventory.NotifyWhenLowAmount = i.NotifyWhenLowAmount;
                response.Inventory.Quantity = i.Quantity;
                response.Inventory.ServiceCodeId = i.ServiceCodeId;

                foreach (ArrangementInventoryInventoryMap index in aiim)
                {
                    string size = GetInventorySize(inventoryList.Where(b => b.InventoryId == index.InventoryId).FirstOrDefault());

                    response.ArrangementList.Add(new ArrangementInventoryItemDTO()
                    {
                        InventoryName = inventoryList.Where(b => b.InventoryId == index.InventoryId).Select(c => c.InventoryName).FirstOrDefault(),
                        ArrangementId = a.ArrangementId,
                        InventoryId = index.InventoryId,
                        InventoryTypeId = index.InventoryTypeId,
                        Quantity = index.Quantity,
                        Size = size
                    });
                }

                List<NotInInventory> notInInventoryList = dbContext.NotInInventory
                    .Where(b => b.ArrangementId == a.ArrangementId).ToList();

                foreach(NotInInventory notInInventory in notInInventoryList)
                {
                    response.NotInInventory.Add(new NotInInventoryDTO()
                    {
                        ArrangementId = notInInventory.ArrangementId,
                        NotInInventoryId = notInInventory.NotInInventoryId,
                        NotInInventoryName = notInInventory.NotInInventoryName,
                        NotInInventoryPrice = notInInventory.NotInInventoryPrice,
                        NotInInventoryQuantity = notInInventory.NotInInventoryQuantity,
                        NotInInventorySize = notInInventory.NotInInventorySize,
                        WorkOrderId = notInInventory.WorkOrderId
                    });
                }

                dbContext.ArrangementImageMap.Where(b => b.ArrangmentId == arrangementId).ToList().ForEach(item =>
                {
                    response.Images.Add(new ImageResponse()
                    {
                        ImageId = item.ImageId,
                        Image = dbContext.Image.Where(c => c.ImageId == item.ImageId).Select(d => d.ImageData).FirstOrDefault()
                    });
                });
            }    

            return response;
        }

        private string GetInventorySize(Inventory inventory)
        {
            string size = String.Empty;
            long tableId = 0;
            switch(inventory.InventoryTypeId)
            {
                case 1: //Orchids
                    tableId = dbContext.InventoryPlantMap.Where(a => a.InventoryId == inventory.InventoryId).Select(b => b.PlantId).FirstOrDefault();
                    size = dbContext.Plant.Where(a => a.PlantId == tableId).Select(b => b.PlantSize).FirstOrDefault();
                    break;
                case 2: //Containers
                    tableId = dbContext.InventoryContainerMap.Where(a => a.InventoryId == inventory.InventoryId).Select(b => b.ContainerId).FirstOrDefault();
                    size = dbContext.Container.Where(a => a.ContainerId == tableId).Select(b => b.ContainerSize).FirstOrDefault();
                    break;
                case 3: //Arrangements
                    
                    break;
                case 4: //Foliage
                    tableId = dbContext.InventoryFoliageMap.Where(a => a.InventoryId == inventory.InventoryId).Select(b => b.FoliageId).FirstOrDefault();
                    size = dbContext.Foliage.Where(a => a.FoliageId == tableId).Select(b => b.FoliageSize).FirstOrDefault();
                    break;
                case 5: //Materials
                    tableId = dbContext.InventoryMaterialMap.Where(a => a.InventoryId == inventory.InventoryId).Select(b => b.MaterialId).FirstOrDefault();
                    size = dbContext.Material.Where(a => a.MaterialId == tableId).Select(b => b.MaterialSize).FirstOrDefault();
                    break;

            }

            
            return size == null ? String.Empty : size;
        }

        public ApiResponse AddImage(AddImageRequest request)
        {
            ApiResponse response = new ApiResponse();

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    Image image = new Image()
                    {
                        ImageData = request.imgBytes
                    };

                    dbContext.Image.Add(image);
                    dbContext.SaveChanges();
                    scope.Complete();
                    response.Id = image.ImageId;
                }
                catch (Exception ex)
                {
                    Dictionary<string, List<string>> msgs = new Dictionary<string, List<string>>();
                    msgs.Add("Error - AddImage", new List<string>() { ex.Message });
                    response.Messages = msgs;
                }
            }

            return response;
        }

        public long AddPlantImage(byte[] imageBytes)
        {
            long plantImageId = 0;

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    Image plantImage = new Image()
                    {
                        ImageData = imageBytes
                    };

                    dbContext.Image.Add(plantImage);
                    dbContext.SaveChanges();
                    scope.Complete();
                    plantImageId = plantImage.ImageId;
                }
                catch (Exception ex)
                {
                    int debug = 1;
                }
            }

            return plantImageId;
        }

        public long AddArrangementImage(AddArrangementImageRequest request)
        {
            long arrangementImageId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {

                    Image arrangementImage = new Image()
                    {
                        ImageData = request.Image
                    };

                    dbContext.Image.Add(arrangementImage);
                    dbContext.SaveChanges();
                   
                    arrangementImageId = arrangementImage.ImageId;

                    ArrangementImageMap map = new ArrangementImageMap()
                    {
                        ArrangmentId = request.ArrangementId,
                        ImageId = arrangementImageId
                    };

                    dbContext.ArrangementImageMap.Add(map);
                    dbContext.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                int debug = 1;
            }

            return arrangementImageId;
        }

        public bool DeleteArrangement(long arrangementId)
        {
            bool arrangementDeleted = false;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    List<ArrangementImageMap> imageMaps =
                        dbContext.ArrangementImageMap.Where(b => b.ArrangmentId == arrangementId).ToList();            

                    dbContext.ArrangementImageMap.RemoveRange(imageMaps);

                    List<ArrangementInventoryInventoryMap> invInvMaps =
                        dbContext.ArrangementInventoryInventoryMap.Where(b => b.ArrangementId == arrangementId).ToList();

                    dbContext.ArrangementInventoryInventoryMap.RemoveRange(invInvMaps);

                    List<ArrangementInventoryMap> invMaps =
                        dbContext.ArrangementInventoryMap.Where(b => b.ArrangementId == arrangementId).ToList();

                    dbContext.ArrangementInventoryMap.RemoveRange(invMaps);

                    List<NotInInventory> notInInventory =
                        dbContext.NotInInventory.Where(b => b.ArrangementId == arrangementId).ToList();

                    dbContext.NotInInventory.RemoveRange();

                    if(dbContext.WorkOrderArrangementMap.Where(b => b.ArrangementId == arrangementId).Any())
                    {
                        WorkOrderArrangementMap woam = dbContext.WorkOrderArrangementMap.Where(b => b.ArrangementId == arrangementId).First();
                        dbContext.WorkOrderArrangementMap.Remove(woam);
                    }

                    Arrangement a = dbContext.Arrangement.Where(b => b.ArrangementId == arrangementId).First();
                    dbContext.Arrangement.Remove(a);

                    dbContext.SaveChanges();
                    arrangementDeleted = true;
                }
            }
            catch(Exception ex)
            {

            }

            return arrangementDeleted;
        }

        public GetVendorResponse GetVendors(GetPersonRequest request)
        {
            GetVendorResponse response = new GetVendorResponse();

            List<long> vendorIds = new List<long>();

            if (request.IsEmpty())
            {
                vendorIds = dbContext.Vendor.Select(a => a.VendorId).ToList();
            }
            else
            {
                if (!String.IsNullOrEmpty(request.FirstName))
                {
                    List<long> x = dbContext.Vendor.Where(a => a.VendorName.Contains(request.FirstName)).Select(b => b.VendorId).ToList();

                    vendorIds = vendorIds.Union(x).ToList();
                }

                if (!String.IsNullOrEmpty(request.PhonePrimary))
                {
                    List<long> x = dbContext.Vendor.Where(a => a.VendorPhone == request.PhonePrimary).Select(b => b.VendorId).ToList();

                    vendorIds = vendorIds.Union(x).ToList();
                }

                if (!String.IsNullOrEmpty(request.Email))
                {
                    List<long> x = dbContext.Vendor.Where(a => a.VendorEmail == request.Email).Select(b => b.VendorId).ToList();

                    vendorIds = vendorIds.Union(x).ToList();
                }
            }

            List<VendorDTO> vendorList = new List<VendorDTO>();

            dbContext.Vendor.Where(a => vendorIds.Contains(a.VendorId)).ToList().ForEach(item =>
            {
                vendorList.Add(new VendorDTO()
                {
                    VendorId = item.VendorId,
                    VendorName = item.VendorName,
                    VendorPhone = item.VendorPhone,
                    VendorEmail = item.VendorEmail,
                });
            });

            List<VendorAddressMap> vendorAdxMap = dbContext.VendorAddressMap.Where(a => vendorIds.Contains(a.VendorId)).ToList();

            List<long> addressIds = vendorAdxMap.Where(a => vendorIds.Contains(a.VendorId)).Select(b => b.AddressId).ToList();

            List<AddressDTO> addresses = new List<AddressDTO>();

            dbContext.Address.Where(a => addressIds.Contains(a.AddressId)).ToList().ForEach(item =>
            {
                addresses.Add(new AddressDTO()
                {
                    address_type_id = item.AddressTypeId.HasValue ? item.AddressTypeId.Value : 0,
                    address_id = item.AddressId,
                    street_address = item.StreetAddress,
                    unit_apt_suite = item.UnitAptSuite,
                    state = item.State,
                    zipcode = item.Zipcode,
                    city = item.City
                });
            });

            foreach (VendorDTO v in vendorList)
            {
                AddressDTO a = null;

                long addressId = vendorAdxMap.Where(b => b.VendorId == v.VendorId).Select(c => c.AddressId).FirstOrDefault();

                if (addressId > 0)
                {
                    a = addresses.Where(b => b.address_id == addressId).FirstOrDefault();

                    v.StreetAddress = a.street_address;
                    v.UnitAptSuite = a.unit_apt_suite;
                    v.City = a.city;
                    v.State = a.state;
                    v.ZipCode = a.zipcode;
                }
            }

            response.VendorList = vendorList;

            return response;
        }

        public GetVendorResponse GetVendorById(long vendorId)
        {
            GetVendorResponse response = new GetVendorResponse();

            Vendor v = dbContext.Vendor.Where(a => a.VendorId == vendorId).FirstOrDefault();

            VendorDTO dto = new VendorDTO();

            dto.VendorName = v.VendorName;
            dto.VendorEmail = v.VendorEmail;
            dto.VendorPhone = v.VendorPhone;

            if(v != null && v.VendorId == vendorId)
            {
                VendorAddressMap vam = dbContext.VendorAddressMap.Where(a => a.VendorId == vendorId).FirstOrDefault();

                if(vam != null && vam.VendorAddressMapId > 0)
                {
                    Address adx = dbContext.Address.Where(a => a.AddressId == vam.AddressId).FirstOrDefault();

                    if(adx != null && adx.AddressId > 0)
                    {
                        dto.StreetAddress = adx.StreetAddress;
                        dto.UnitAptSuite = adx.UnitAptSuite;
                        dto.City = adx.City;
                        dto.State = adx.State;
                        dto.ZipCode = adx.Zipcode;
                        dto.VendorAddressMapId = vam.VendorAddressMapId;
                    }
                }
            }

            response.VendorList.Add(dto);

            return response;
        }

        public long AddVendor(AddVendorRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {

                    Vendor v = new Vendor()
                    {
                        VendorName = request.Vendor.VendorName,
                        VendorPhone = request.Vendor.VendorPhone,
                        VendorEmail = request.Vendor.VendorEmail
                    };

                    dbContext.Vendor.Add(v);

                    if (!String.IsNullOrEmpty(request.Vendor.StreetAddress) && !String.IsNullOrEmpty(request.Vendor.City) &&
                        !String.IsNullOrEmpty(request.Vendor.City) && !String.IsNullOrEmpty(request.Vendor.State) && !String.IsNullOrEmpty(request.Vendor.ZipCode))
                    {
                        Address a = new Address()
                        {
                            AddressTypeId = 4,
                            StreetAddress = request.Vendor.StreetAddress,
                            UnitAptSuite = request.Vendor.UnitAptSuite,
                            City = request.Vendor.City,
                            State = request.Vendor.State,
                            Zipcode = request.Vendor.ZipCode
                        };

                        dbContext.Address.Add(a);

                        dbContext.SaveChanges();

                        VendorAddressMap vam = new VendorAddressMap()
                        {
                            AddressId = a.AddressId,
                            VendorId = v.VendorId
                        };

                        dbContext.VendorAddressMap.Add(vam);
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    newId = v.VendorId;
                }
            }
            catch (Exception ex)
            {
                int debug = 1;
            }

            return newId;
        }

        public long AddCustomer(AddCustomerRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    Person p = new Person()
                    {
                        FirstName = request.Customer.Person.first_name,
                        LastName = request.Customer.Person.last_name,
                        Email = request.Customer.Person.email,
                        PhonePrimary = request.Customer.Person.phone_primary,
                        PhoneAlt = request.Customer.Person.phone_alt,
                        UserId = 1
                    };

                    dbContext.Person.Add(p);

                    Address a = new Address()
                    {
                        AddressTypeId = 1,
                        StreetAddress = request.Customer.Address.street_address,
                        UnitAptSuite = request.Customer.Address.unit_apt_suite,
                        City = request.Customer.Address.city,
                        State = request.Customer.Address.state,
                        Zipcode = request.Customer.Address.zipcode
                    };

                    dbContext.Address.Add(a);

                    PersonAddressMap map = new PersonAddressMap()
                    {
                        AddresId = a.AddressId,
                        PersonId = p.PersonId,
                    };

                    dbContext.PersonAddressMap.Add(map);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = p.PersonId;
                }
            }
            catch(Exception ex)
            {

            }

            return newId;
        }

        public long AddShipment(AddShipmentRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {

                    Shipment s = new Shipment()
                    {
                        ShipmentDate = request.ShipmentDTO.ShipmentDate,
                        VendorId = request.ShipmentDTO.VendorId,
                        ReceiverId = request.ShipmentDTO.ReceiverId,
                        Comments = request.ShipmentDTO.Comments
                    };

                    dbContext.Shipment.Add(s);
                    dbContext.SaveChanges();

                    Dictionary<long, int> inventoryQuantities = new Dictionary<long, int>();

                    List<ShipmentInventoryMap> mapList = new List<ShipmentInventoryMap>();

                    foreach(ShipmentInventoryMapDTO map in request.ShipmentInventoryMap)
                    {
                        if(inventoryQuantities.ContainsKey(map.InventoryId))
                        {
                            inventoryQuantities[map.InventoryId] += map.Quantity;
                        }
                        else
                        {
                            inventoryQuantities.Add(map.InventoryId, map.Quantity);
                        }

                        ShipmentInventoryMap sim = new ShipmentInventoryMap()
                        {
                            InventoryId = map.InventoryId,
                            Quantity = map.Quantity,
                            ShipmentId = s.ShipmentId
                        };

                        dbContext.ShipmentInventoryMap.Add(sim);
                        dbContext.SaveChanges();

                       foreach(ShipmentInventoryImageMapDTO a in map.ShipmentInventoryImageMap)
                       {
                            if(a.ImageData != null && a.ImageData.Length > 0)
                            {
                                Image i = new Image()
                                {
                                    ImageData = a.ImageData
                                };

                                dbContext.Image.Add(i);
                                dbContext.SaveChanges();

                                ShipmentInventoryImageMap siim = new ShipmentInventoryImageMap();

                                siim.ShipmentInventoryMapId = sim.ShipmentInventoryMapId;
                                siim.ImageId = i.ImageId;

                                dbContext.ShipmentInventoryImageMap.Add(siim);
                                dbContext.SaveChanges();
                            }
                       }
                    }

                    foreach(var wtf in inventoryQuantities)
                    {
                        Inventory i = dbContext.Inventory.Where(a => a.InventoryId == wtf.Key).FirstOrDefault();
                        if(i != null && i.InventoryId == wtf.Key)
                        {
                            i.Quantity += wtf.Value;
                        }
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    newId = s.ShipmentId;
                }
            }
            catch (Exception ex)
            {
                int debug = 1;
            }

            return newId;
        }

        public long UpdateShipment(AddShipmentRequest request)
        {
            long updatedShipmentId = request.ShipmentDTO.ShipmentId;

            Shipment s = dbContext.Shipment.Where(a => a.ShipmentId == request.ShipmentDTO.ShipmentId).FirstOrDefault();

            if(s != null && s.ShipmentId == request.ShipmentDTO.ShipmentId)
            {

            }

            return updatedShipmentId;
        }

        public ShipmentInventoryDTO GetShipment(long shipmentId)
        {
            ShipmentInventoryDTO response = new ShipmentInventoryDTO();

            Shipment s = dbContext.Shipment.Where(a => a.ShipmentId == shipmentId).FirstOrDefault();

            if (s != null && s.ShipmentId == shipmentId)
            {
                List<ShipmentInventoryMapDTO> inventoryMap = new List<ShipmentInventoryMapDTO>();

                dbContext.ShipmentInventoryMap.Where(b => b.ShipmentId == s.ShipmentId).ToList().ForEach(map =>
                {
                    List<ShipmentInventoryImageMapDTO> inventoryImageMap = new List<ShipmentInventoryImageMapDTO>();
                    dbContext.ShipmentInventoryImageMap.Where(c => c.ShipmentInventoryMapId == map.ShipmentInventoryMapId).ToList().ForEach(imageMap =>
                    {
                        Image i = dbContext.Image.Where(d => d.ImageId == imageMap.ImageId).First();

                        inventoryImageMap.Add(new ShipmentInventoryImageMapDTO()
                        {
                            ShipmentInventoryImageMapId = imageMap.ShipmentInventoryImageMapId,
                            ShipmentInventoryMapId = imageMap.ShipmentInventoryMapId,
                            ImageId = imageMap.ImageId,
                            ImageData = (i != null && i.ImageData != null) ? i.ImageData : null
                        });
                    });

                    inventoryMap.Add(new ShipmentInventoryMapDTO()
                    {
                        InventoryId = map.InventoryId,
                        InventoryName = dbContext.Inventory.Where(c => c.InventoryId == map.InventoryId).Select(d => d.InventoryName).First(),
                        Quantity = map.Quantity,
                        ShipmentId = s.ShipmentId,
                        ShipmentInventoryMapId = map.ShipmentInventoryMapId,
                        ShipmentInventoryImageMap = inventoryImageMap
                    });
                });

                ShipmentDTO shipment = new ShipmentDTO()
                {
                    ShipmentId = s.ShipmentId,
                    ShipmentDate = s.ShipmentDate,
                    VendorId = s.VendorId,
                    ReceiverId = s.ReceiverId,
                    VendorName = dbContext.Vendor.Where(e => e.VendorId == s.VendorId).Select(f => f.VendorName).First(),
                    Comments = s.Comments
                };

                response.Shipment = shipment;
                response.ShipmentInventoryMap = inventoryMap;
            }

            return response;
        }
        public GetShipmentResponse GetShipments(ShipmentFilter filter)
        {
            GetShipmentResponse shipmentResponse = new GetShipmentResponse();

            dbContext.Shipment.Where(a => a.ShipmentDate >= filter.FromDate && a.ShipmentDate <= filter.ToDate).ToList().ForEach(item =>
            {
                List<ShipmentInventoryMapDTO> inventoryMap = new List<ShipmentInventoryMapDTO>();

                dbContext.ShipmentInventoryMap.Where(b => b.ShipmentId == item.ShipmentId).ToList().ForEach(map =>
                {
                    List<ShipmentInventoryImageMapDTO> inventoryImageMap = new List<ShipmentInventoryImageMapDTO>();
                    dbContext.ShipmentInventoryImageMap.Where(c => c.ShipmentInventoryMapId == map.ShipmentInventoryMapId).ToList().ForEach(imageMap =>
                    {
                        Image i = dbContext.Image.Where(d => d.ImageId == imageMap.ImageId).First();

                        inventoryImageMap.Add(new ShipmentInventoryImageMapDTO()
                        {
                            ShipmentInventoryImageMapId = imageMap.ShipmentInventoryImageMapId,
                            ShipmentInventoryMapId = imageMap.ShipmentInventoryMapId,
                            ImageId = imageMap.ImageId,
                            ImageData = (i != null && i.ImageData != null) ? i.ImageData : null
                        });
                    });

                    inventoryMap.Add(new ShipmentInventoryMapDTO()
                    {
                        InventoryId = map.InventoryId,
                        InventoryName = dbContext.Inventory.Where(c => c.InventoryId == map.InventoryId).Select(d => d.InventoryName).First(),
                        Quantity = map.Quantity,
                        ShipmentId = item.ShipmentId,
                        ShipmentInventoryMapId = map.ShipmentInventoryMapId,
                        ShipmentInventoryImageMap = inventoryImageMap
                    });
                });

                ShipmentDTO shipment = new ShipmentDTO()
                {
                    ShipmentId = item.ShipmentId,
                    ShipmentDate = item.ShipmentDate,
                    VendorId = item.VendorId,
                    ReceiverId = item.ReceiverId,
                    VendorName = dbContext.Vendor.Where(e => e.VendorId == item.VendorId).Select(f => f.VendorName).First(),
                    Comments = item.Comments
                };

                shipmentResponse.ShipmentList.Add(new ShipmentInventoryDTO(shipment, inventoryMap));
            });

            return shipmentResponse;
        }

        public long AddWorkOrderPayment(WorkOrderPaymentDTO workOrderPayment)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {

                    WorkOrderPayment w = new WorkOrderPayment()
                    {
                        WorkOrderId = workOrderPayment.WorkOrderId,
                        WorkOrderPaymentAmount = workOrderPayment.WorkOrderPaymentAmount,
                        WorkOrderPaymentCreditCardConfirmation = workOrderPayment.WorkOrderPaymentCreditCardConfirmation,
                        WorkOrderPaymentTax = workOrderPayment.WorkOrderPaymentTax,
                        WorkOrderPaymentType = workOrderPayment.WorkOrderPaymentType,
                        DiscountType = workOrderPayment.DiscountType,
                        DiscountAmount = workOrderPayment.DiscountAmount
                    };

                    dbContext.WorkOrderPayment.Add(w);

                    WorkOrder wo = dbContext.WorkOrder.Where(a => a.WorkOrderId == workOrderPayment.WorkOrderId).First();
                    wo.Paid = true;

                    dbContext.SaveChanges();

                    scope.Complete();
                    newId = w.WorkOrderPaymentId;
                }
            }
            catch(Exception ex)
            {

            }

            return newId;
        }

        public WorkOrderPaymentDTO GetWorkOrderPayment(long workOrderId)
        {
            WorkOrderPaymentDTO workOrderPayment = new WorkOrderPaymentDTO();

            try
            {
                WorkOrderPayment wop = dbContext.WorkOrderPayment.Where(a => a.WorkOrderId == workOrderId).FirstOrDefault();

                if (wop != null && wop.WorkOrderId == workOrderId)
                {
                    workOrderPayment.WorkOrderId = wop.WorkOrderId;
                    workOrderPayment.WorkOrderPaymentAmount = wop.WorkOrderPaymentAmount;
                    workOrderPayment.WorkOrderPaymentCreditCardConfirmation = wop.WorkOrderPaymentCreditCardConfirmation;
                    workOrderPayment.WorkOrderPaymentId = wop.WorkOrderPaymentId;
                    workOrderPayment.WorkOrderPaymentTax = wop.WorkOrderPaymentTax;
                    workOrderPayment.WorkOrderPaymentType = wop.WorkOrderPaymentType;
                    workOrderPayment.DiscountType = wop.DiscountType;
                    workOrderPayment.DiscountAmount = wop.DiscountAmount;
                }
            }
            catch (Exception ex)
            {

            }

            return workOrderPayment;
        }

        public List<long> GetWorkOrderImageIds(long workOrderId)
        {
            List<long> imageIds = new List<long>();

            try
            {
                dbContext.WorkOrderImageMap.Where(a => a.WorkOrderId == workOrderId).ToList().ForEach(item => 
                {
                    imageIds.Add(item.ImageId);
                });

            }
            catch (Exception ex)
            {

            }

            return imageIds;
        }

        public bool MarkWorkOrderPaid(long workOrderId)
        {
            bool success = false;
            try
            {
                WorkOrder wo = dbContext.WorkOrder.Where(a => a.WorkOrderId == workOrderId).FirstOrDefault();

                if (wo != null && wo.WorkOrderId == workOrderId)
                {
                      using (var scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        wo.Paid = true;
       
                        dbContext.SaveChanges();
                        scope.Complete();
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return success;
        }

        /// <summary>
        /// When a work order is saved, inventory quantities are adjusted
        /// If a work order has been saved, but not paid for, a work order
        /// can be cancelled. On cancellation, "re-add" the work order inventory
        /// quantities IF the work order has NOT been paid
        /// </summary>
        /// <param name="workOrderId"></param>
        /// <returns></returns>
        public long CancelWorkOrder(long workOrderId)
        {
            long adjustedWorkOrderId = 0;

            try
            {
                WorkOrder wo = dbContext.WorkOrder.Where(a => a.WorkOrderId == workOrderId).FirstOrDefault();

                if (wo != null && wo.WorkOrderId == workOrderId && !wo.Paid)
                {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        wo.IsCancelled = true;

                        List<WorkOrderInventoryMap> woim = dbContext.WorkOrderInventoryMap.Where(a => a.WorkOrderId == workOrderId).ToList();

                        foreach(WorkOrderInventoryMap map in woim)
                        {
                            dbContext.Inventory.Where(b => b.InventoryId == map.InventoryId).First().Quantity += map.Quantity;
                        }

                        dbContext.SaveChanges();
                        scope.Complete();
                        adjustedWorkOrderId = workOrderId;
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return adjustedWorkOrderId;
        }

        public long AddWorkOrder(AddWorkOrderRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {

                    WorkOrder w = new WorkOrder()
                    {
                        PersonInitiator = request.WorkOrder.Seller,
                        PersonReceiver = request.WorkOrder.Buyer,
                        PersonDelivery = request.WorkOrder.DeliveredBy,
                        DeliveryReceiver = request.WorkOrder.DeliverTo,
                        CreateDate = request.WorkOrder.CreateDate,
                        Comments = request.WorkOrder.Comments,
                        DeliveryDate = request.WorkOrder.DeliveryDate,
                        IsSiteService = request.WorkOrder.IsSiteService,
                        IsDelivery = request.WorkOrder.IsDelivery,
                        DeliveryType = request.WorkOrder.DeliveryType,
                        IsCancelled = request.WorkOrder.IsCancelled,
                        OrderType = request.WorkOrder.OrderType,
                        CustomerId = request.WorkOrder.CustomerId,
                        SellerId = request.WorkOrder.SellerId,
                        DeliveryRecipientId = request.WorkOrder.DeliveryRecipientId,
                        DeliveryUserId = request.WorkOrder.DeliveryUserId,
                        Delivered = request.WorkOrder.Delivered
                    };

                    dbContext.WorkOrder.Add(w);
                    dbContext.SaveChanges();

                    Dictionary<long, int> inventoryQuantities = new Dictionary<long, int>();

                    List<WorkOrderInventoryMap> mapList = new List<WorkOrderInventoryMap>();

                    //to accomodate Melissa's UI requests, Arrangements are grouped with spacing
                    //and header rows - don't save these
                    foreach (WorkOrderInventoryMapDTO map in request.WorkOrderInventoryMap)
                    {
                        if(map.InventoryId == 0 && map.Quantity == 0 && map.GroupId !=0)
                        {
                            continue;
                        }

                        if ((!String.IsNullOrEmpty(map.NotInInventoryName) && !String.IsNullOrEmpty(map.NotInInventorySize) && map.NotInInventoryPrice > 0) ||
                            map.InventoryId == 0)
                        {
                            map.InventoryId = 387; //fk inventory_id  "not In Inventory" uses constant
                        }
                        else
                        {
                            if (inventoryQuantities.ContainsKey(map.InventoryId))
                            {
                                inventoryQuantities[map.InventoryId] += map.Quantity;
                            }
                            else
                            {
                                inventoryQuantities.Add(map.InventoryId, map.Quantity);
                            }
                        }
                        

                        mapList.Add(new WorkOrderInventoryMap()
                        {
                            InventoryId = map.InventoryId,
                            Quantity = map.Quantity,
                            WorkOrderId = w.WorkOrderId,
                            GroupId = map.GroupId,
                        });
                    }

                    dbContext.WorkOrderInventoryMap.AddRange(mapList);

                    List<NotInInventory> notInInventoryList = new List<NotInInventory>();

                    foreach (NotInInventoryDTO notInInventory in request.NotInInventory)
                    {
                        notInInventoryList.Add(new NotInInventory()
                        {
                            WorkOrderId = w.WorkOrderId,
                            ArrangementId = notInInventory.ArrangementId,
                            NotInInventoryName = notInInventory.NotInInventoryName,
                            NotInInventorySize = notInInventory.NotInInventorySize,
                            NotInInventoryQuantity = notInInventory.NotInInventoryQuantity,
                            NotInInventoryPrice = notInInventory.NotInInventoryPrice
                        });
                    }

                    dbContext.NotInInventory.AddRange(notInInventoryList);



                    if (request.ImageMap != null && request.ImageMap.Count > 0)
                    {
                        List<WorkOrderImageMap> imageMapList = new List<WorkOrderImageMap>();

                        foreach (WorkOrderImageMapDTO mapDTO in request.ImageMap)
                        {
                            Image i = new Image();
                            i.ImageData = mapDTO.ImageData;
                            dbContext.Add(i);
                            dbContext.SaveChanges();

                            imageMapList.Add(new WorkOrderImageMap()
                            {
                                WorkOrderId = w.WorkOrderId,
                                ImageId = i.ImageId
                            });
                        }

                        dbContext.WorkOrderImageMap.AddRange(imageMapList);
                    }

                    foreach(AddArrangementRequest ar in request.Arrangements)
                    {
                        if(!ar.Arrangement.ServiceCodeId.HasValue || ar.Arrangement.ServiceCodeId == 0)
                        {
                            ar.Arrangement.ServiceCodeId = 365;   //temp constant 
                        }
                        //save each
                        Arrangement a = new Arrangement()
                        {
                            ArrangementName = ar.Arrangement.ArrangementName,
                            Container = ar.Arrangement.Container,
                            CustomerContainerId = ar.Arrangement.CustomerContainerId,
                            DesignerName = ar.Arrangement.DesignerName,
                            GiftMessage = ar.Arrangement.GiftMessage,
                            IsGift = ar.Arrangement.IsGift,
                            LocationName = ar.Arrangement.LocationName,
                            ServiceCodeId = ar.Arrangement.ServiceCodeId,
                            UpdateDate = DateTime.Now,
                            _180or360 = ar.Arrangement._180or360
                        };

                        dbContext.Arrangement.Add(a);
                        dbContext.SaveChanges();

                        if(ar.Inventory.ServiceCodeId == 0)
                        {
                            ar.Inventory.ServiceCodeId = 365;  // use temp constant
                        }

                        //add Inventory
                        Inventory i = new Inventory()
                        {
                            NotifyWhenLowAmount = ar.Inventory.NotifyWhenLowAmount,
                            InventoryName = ar.Inventory.InventoryName,
                            InventoryTypeId = ar.Inventory.InventoryTypeId,
                            ServiceCodeId = ar.Inventory.ServiceCodeId,
                            Quantity = ar.Inventory.Quantity
                        };
                        dbContext.Inventory.Add(i);
                        dbContext.SaveChanges();

                        //add ArrangementInventoryMap - add new arrangement id
                        ArrangementInventoryMap aiMap = new ArrangementInventoryMap()
                        {
                            ArrangementId = a.ArrangementId,
                            InventoryId = i.InventoryId
                        };

                        dbContext.ArrangementInventoryMap.Add(aiMap);

                        //add work orderid and arrangement id to work_order_arrangement_map
                        WorkOrderArrangementMap woaMap = new WorkOrderArrangementMap()
                        {
                            WorkOrderId = w.WorkOrderId,
                            ArrangementId = a.ArrangementId
                        };

                        dbContext.WorkOrderArrangementMap.Add(woaMap);

                        foreach(ArrangementInventoryDTO aid in ar.ArrangementInventory)
                        {
                            if (aid.InventoryId == 0)
                                continue;

                            ArrangementInventoryInventoryMap aiim = new ArrangementInventoryInventoryMap()
                            {
                               ArrangementId = a.ArrangementId,
                               InventoryId = aid.InventoryId,
                               InventoryTypeId = aid.InventoryTypeId,
                               Quantity = aid.Quantity
                            };

                            dbContext.ArrangementInventoryInventoryMap.Add(aiim);
                        }

                        dbContext.SaveChanges();

                        //map the "Not In Inventory" items
                        foreach(NotInInventoryDTO niid in ar.NotInInventory)
                        {
                            NotInInventory nid = new NotInInventory()
                            {
                                ArrangementId = a.ArrangementId,
                                NotInInventoryName = niid.NotInInventoryName,
                                NotInInventoryPrice = niid.NotInInventoryPrice,
                                NotInInventoryQuantity = niid.NotInInventoryQuantity,
                                NotInInventorySize = niid.NotInInventorySize,
                                WorkOrderId = w.WorkOrderId
                            };

                            dbContext.NotInInventory.Add(nid);
                        }

                        //modify GetWorkOrder to get the above data

                        dbContext.SaveChanges();
                    }


                    dbContext.SaveChanges();
                    scope.Complete();
                    newId = w.WorkOrderId;
                }
            }
            catch (Exception ex)
            {
                int debug = 1;
            }

            return newId;
        }



        public long AddWorkOrderImage(AddWorkOrderImageRequest request)
        {
            long newId = 0;
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    if (request.ImageId == 0)
                    {
                        Image img = new Image()
                        {
                            ImageData = request.Image
                        };

                        dbContext.Image.Add(img);

                        dbContext.SaveChanges();

                        WorkOrderImageMap imageMap = new WorkOrderImageMap()
                        {
                            WorkOrderId = request.WorkOrderId,
                            ImageId = img.ImageId
                        };

                        dbContext.WorkOrderImageMap.Add(imageMap);
                        dbContext.SaveChanges();
                        scope.Complete();
                        newId = imageMap.WorkOrderImageMapId;
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return newId;
        }

        public long UpdateWorkOrder(AddWorkOrderRequest request)
        {
            long updatedWorkOrderId = request.WorkOrder.WorkOrderId;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    WorkOrder wo = dbContext.WorkOrder.Where(a => a.WorkOrderId == updatedWorkOrderId).First();

                    if(wo != null && wo.WorkOrderId == updatedWorkOrderId)
                    {
                        wo.PersonInitiator = request.WorkOrder.Seller;
                        wo.PersonReceiver = request.WorkOrder.Buyer;
                        wo.PersonDelivery = request.WorkOrder.DeliveredBy;
                        wo.DeliveryReceiver = request.WorkOrder.DeliverTo;
                        wo.UpdateDate = request.WorkOrder.CreateDate;
                        wo.Comments = request.WorkOrder.Comments;
                        wo.DeliveryDate = request.WorkOrder.DeliveryDate;
                        wo.IsSiteService = request.WorkOrder.IsSiteService;
                        wo.IsDelivery = request.WorkOrder.IsDelivery;
                        wo.DeliveryType = request.WorkOrder.DeliveryType;
                        wo.IsCancelled = request.WorkOrder.IsCancelled;
                        wo.OrderType = request.WorkOrder.OrderType;
                        wo.CustomerId = request.WorkOrder.CustomerId;
                        wo.SellerId = request.WorkOrder.SellerId;
                        wo.DeliveryRecipientId = request.WorkOrder.DeliveryRecipientId;
                        wo.DeliveryUserId = request.WorkOrder.DeliveryUserId;
                        wo.Delivered = request.WorkOrder.Delivered;
                    }

                    //can't change inventory items if this has already been paid
                    if (!request.WorkOrder.Paid)
                    {
                        //delete old inventory items 
                        List<WorkOrderInventoryMap> oldInventoryMaps =
                            dbContext.WorkOrderInventoryMap.Where(a => a.WorkOrderId == request.WorkOrder.WorkOrderId).ToList();

                        dbContext.RemoveRange(oldInventoryMaps);

                        //add all from new list
                        List<WorkOrderInventoryMap> mapList = new List<WorkOrderInventoryMap>();
                        foreach (WorkOrderInventoryMapDTO map in request.WorkOrderInventoryMap)
                        {
                            mapList.Add(new WorkOrderInventoryMap()
                            {
                                InventoryId = map.InventoryId,
                                Quantity = map.Quantity,
                                WorkOrderId = updatedWorkOrderId,
                            });
                        }

                        dbContext.WorkOrderInventoryMap.AddRange(mapList);

                        List<NotInInventory> update = dbContext.NotInInventory.Where(a => a.WorkOrderId == request.WorkOrder.WorkOrderId).ToList();

                        dbContext.NotInInventory.RemoveRange(update);

                        dbContext.SaveChanges();

                        update.Clear();
                        foreach (NotInInventoryDTO d in request.NotInInventory)
                        {
                            update.Add(new NotInInventory()
                            {
                                WorkOrderId = updatedWorkOrderId,
                                ArrangementId = 0,
                                NotInInventoryName = d.NotInInventoryName,
                                NotInInventoryPrice = d.NotInInventoryPrice,
                                NotInInventoryQuantity = d.NotInInventoryQuantity,
                                NotInInventorySize = d.NotInInventorySize
                            });
                        }

                        dbContext.NotInInventory.AddRange(update);

                        List<long> deleteArrangementIds = new List<long>();

                        List<long> requestArrangementIds = request.Arrangements.Select(a => a.Arrangement.ArrangementId).ToList();

                        dbContext.WorkOrderArrangementMap.Where(a => a.WorkOrderId == request.WorkOrder.WorkOrderId).Select(b => b.ArrangementId).ToList().ForEach( arrangementId => 
                        { 
                            if(!requestArrangementIds.Contains(arrangementId))
                            {
                                deleteArrangementIds.Add(arrangementId);
                            }
                        });

                        if(deleteArrangementIds.Count > 0)
                        {
                            List<WorkOrderArrangementMap> woam = dbContext.WorkOrderArrangementMap.Where(a => deleteArrangementIds.Contains(a.ArrangementId)).ToList();
                            dbContext.WorkOrderArrangementMap.RemoveRange(woam);

                            foreach(long deleteId in deleteArrangementIds)
                            {
                                DeleteArrangement(deleteId);
                            }
                        }

                        foreach (AddArrangementRequest aaRequest in request.Arrangements)
                        {
                            long newOrUpdatedId = 0;
                            if (aaRequest.Arrangement.ArrangementId <= 0)
                            {
                                newOrUpdatedId = AddArrangement(aaRequest);

                                WorkOrderArrangementMap woam2 = new WorkOrderArrangementMap()
                                {
                                    WorkOrderId = updatedWorkOrderId,
                                    ArrangementId = newOrUpdatedId
                                };

                                dbContext.WorkOrderArrangementMap.Add(woam2);
                            }
                            else
                            {
                                newOrUpdatedId = UpdateArrangement(aaRequest);
                            }
                        }
                    }

                    if (request.ImageMap != null && request.ImageMap.Count > 0)
                    {
                        //delete  old image maps
                        List<WorkOrderImageMap> imageMaps = dbContext.WorkOrderImageMap.Where(a => a.WorkOrderId == request.WorkOrder.WorkOrderId).ToList();

                        List<long> imageIds = imageMaps.Select(a => a.ImageId).ToList();

                        dbContext.RemoveRange(imageMaps);

                        //delete old images
                        List<Image> oldImages = dbContext.Image.Where(a => imageIds.Contains(a.ImageId)).ToList();

                        dbContext.RemoveRange(oldImages);

                        //add image list
                        imageMaps.Clear();

                        foreach (WorkOrderImageMapDTO mapDTO in request.ImageMap)
                        {
                            Image i = new Image();
                            i.ImageData = mapDTO.ImageData;
                            dbContext.Add(i);
                            dbContext.SaveChanges();

                            imageMaps.Add(new WorkOrderImageMap()
                            {
                                WorkOrderId = updatedWorkOrderId,
                                ImageId = i.ImageId
                            });
                        }

                        dbContext.WorkOrderImageMap.AddRange(imageMaps);
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                updatedWorkOrderId = 0;
            }
            return updatedWorkOrderId;
        }
        public long DoesPersonExist(PersonDTO person)
        {
            long personId = 0;

            Person p = dbContext.Person.Where(a => a.FirstName == person.first_name && a.LastName == person.last_name && a.PhonePrimary == person.phone_primary).FirstOrDefault();

            if(p != null && p.PersonId > 0)
            {
                personId = p.PersonId;
            }

            return personId;
        }

        public long ImportPerson(ImportPersonRequest request)
        {
            long personId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    Person p = new Person()
                    {
                        FirstName = request.Person.first_name,
                        LastName = request.Person.last_name,
                        PhonePrimary = request.Person.phone_primary,
                        PhoneAlt = request.Person.phone_alt,
                        LastUpdated = DateTime.Now,
                        Email = request.Person.email,
                        UserId = 1  // fix this - person should not depend on FK_user
                    };

                    dbContext.Person.Add(p);
                    dbContext.SaveChanges();
                    personId = p.PersonId;

                    if(!String.IsNullOrEmpty(request.Address.street_address))
                    {
                        Address a = new Address()
                        {
                            State ="FL",
                            StreetAddress = request.Address.street_address,
                            UnitAptSuite = request.Address.unit_apt_suite,
                            Zipcode = request.Address.zipcode,
                            AddressTypeId = request.Address.address_type_id,
                            City = request.Address.city
                        };

                        dbContext.Address.Add(a);
                        dbContext.SaveChanges();
                        long addressId = a.AddressId;

                        if(personId > 0 && addressId > 0)
                        {
                            PersonAddressMap pam = new PersonAddressMap()
                            {
                                AddresId = addressId,
                                PersonId = personId
                            };

                            dbContext.PersonAddressMap.Add(pam);
                            dbContext.SaveChanges();
                        }
                    }

                    scope.Complete();
                }
  
            }
            catch (Exception ex)
            {

            }

            return personId;
        }

        public GetPersonResponse GetPerson(GetPersonRequest request)
        {
            GetPersonResponse response = new GetPersonResponse();

            List<long> personIds = new List<long>();

            if(request.IsEmpty())
            {
                personIds = dbContext.Person.Select(a => a.PersonId).ToList();
            }
            else
            {
                if (request.PersonId != 0)
                {
                    personIds.Add(request.PersonId);
                }
                else
                {
                    if (!String.IsNullOrEmpty(request.FirstName))
                    {
                        List<long> x = dbContext.Person.Where(a => a.FirstName.Contains(request.FirstName)).Select(b => b.PersonId).ToList();

                        personIds = personIds.Union(x).ToList();
                    }

                    if (!String.IsNullOrEmpty(request.LastName))
                    {
                        List<long> x = dbContext.Person.Where(a => a.LastName.Contains(request.LastName)).Select(b => b.PersonId).ToList();

                        personIds = personIds.Union(x).ToList();
                    }

                    if (!String.IsNullOrEmpty(request.PhonePrimary))
                    {
                        List<long> x = dbContext.Person.Where(a => a.PhonePrimary == request.PhonePrimary || a.PhonePrimary.Contains(request.PhonePrimary)).Select(b => b.PersonId).ToList();

                        personIds = personIds.Union(x).ToList();
                    }

                    if (!String.IsNullOrEmpty(request.PhoneAlt))
                    {
                        List<long> x = dbContext.Person.Where(a => a.PhoneAlt.Contains(request.PhoneAlt)).Select(b => b.PersonId).ToList();

                        personIds = personIds.Union(x).ToList();
                    }

                    if (!String.IsNullOrEmpty(request.Email))
                    {
                        List<long> x = dbContext.Person.Where(a => a.Email.Contains(request.Email)).Select(b => b.PersonId).ToList();

                        personIds = personIds.Union(x).ToList();
                    }

                    if (!String.IsNullOrEmpty(request.Address))
                    {
                        List<long> adxIds = dbContext.Address.Where(a => a.StreetAddress.Contains(request.Address)).Select(b => b.AddressId).ToList();

                        List<long> pIds = dbContext.PersonAddressMap.Where(a => adxIds.Contains(a.AddresId)).Select(b => b.PersonId).ToList();

                        personIds = personIds.Union(pIds).ToList();
                    }

                    if (!String.IsNullOrEmpty(request.ZipCode))
                    {
                        List<long> adxIds = dbContext.Address.Where(a => a.Zipcode.Contains(request.ZipCode)).Select(b => b.AddressId).ToList();

                        List<long> pIds = dbContext.PersonAddressMap.Where(a => adxIds.Contains(a.AddresId)).Select(b => b.PersonId).ToList();

                        personIds = personIds.Union(pIds).ToList();
                    }
                }
            }

            List<PersonDTO> personList = new List<PersonDTO>();

            dbContext.Person.Where(a => personIds.Contains(a.PersonId)).ToList().ForEach(item =>
            {
                personList.Add(new PersonDTO()
                {
                    person_id = item.PersonId,
                    first_name = item.FirstName,
                    last_name = item.LastName,
                    phone_primary = item.PhonePrimary,
                    phone_alt = item.PhoneAlt,
                    email = item.Email,
                    last_updated = item.LastUpdated
                });
            });

            List<PersonAddressMap> personAdxMap = dbContext.PersonAddressMap.Where(a => personIds.Contains(a.PersonId)).ToList();

            List<long> addressIds = personAdxMap.Where(a => personIds.Contains(a.PersonId)).Select(b => b.AddresId).ToList();

            List<AddressDTO> addresses = new List<AddressDTO>();

            dbContext.Address.Where(a => addressIds.Contains(a.AddressId)).ToList().ForEach(item =>
            {
                addresses.Add(new AddressDTO()
                {
                    address_type_id = item.AddressTypeId.HasValue ? item.AddressTypeId.Value : 0,
                    address_id = item.AddressId,
                    street_address = item.StreetAddress,
                    unit_apt_suite = item.UnitAptSuite,
                    state = item.State,
                    zipcode = item.Zipcode,
                    city = item.City
                });       
            });

            foreach(PersonDTO p in personList)
            {
                AddressDTO a = null;

                long addressId = personAdxMap.Where(b => b.PersonId == p.person_id).Select(c => c.AddresId).FirstOrDefault();

                if (addressId != null && addressId > 0)
                {
                    a = addresses.Where(b => b.address_id == addressId).FirstOrDefault();
                }

                response.PersonAndAddress.Add(new PersonAndAddressDTO(p, a));
            }

            return response;
        }

        #region Foliage
        public long FoliageExists(FoliageDTO foliageDTO)
        {
            long foliageId = 0;

            Foliage f = dbContext.Foliage.Where(a => a.FoliageName == foliageDTO.FoliageName && a.FoliageTypeId == foliageDTO.FoliageTypeId).FirstOrDefault();

            if (f != null && f.FoliageId > 0)
            {
                foliageId = f.FoliageId;
            }

            return foliageId;
        }

        public long FoliageNameExists(string foliageName)
        {
            long foliageNameId = 0;

            FoliageName folName = dbContext.FoliageName.Where(a => a.Name == foliageName).FirstOrDefault();

            if (folName != null && folName.FoliageNameId > 0)
            {
                foliageNameId = folName.FoliageNameId;
            }

            return foliageNameId;
        }

        public long FoliageTypeExists(string typeName)
        {
            long foliageTypeId = 0;

            FoliageType folType = dbContext.FoliageType.Where(a => a.FoliageTypeName == typeName).FirstOrDefault();

            if (folType != null && folType.FoliageTypeId > 0)
            {
                foliageTypeId = folType.FoliageTypeId;
            }

            return foliageTypeId;
        }

        public bool FoliageNameIsNotUnique(string foliageName)
        {
            bool notUnique = false;

            Foliage f = dbContext.Foliage.Where(a => a.FoliageName == foliageName).FirstOrDefault();

            if (f != null && f.FoliageId > 0)
            {
                notUnique = true;
            }

            return notUnique;
        }
        public long AddFoliageType(AddFoliageTypeRequest request)
        {
            long newId = 0;
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    FoliageType foliageType = new FoliageType()
                    {
                        FoliageTypeName = request.FoliageTypeName,
                    };

                    dbContext.FoliageType.Add(foliageType);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = foliageType.FoliageTypeId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }

        public long AddFoliageName(AddFoliageNameRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    FoliageName foliageName = new FoliageName()
                    {
                        Name = request.FoliageName,
                        FoliageTypeId = request.FoliageTypeId
                    };

                    dbContext.FoliageName.Add(foliageName);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = foliageName.FoliageNameId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }

        public long AddFoliage(AddFoliageRequest request)
        {
            long foliage_id = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    Foliage f = new Foliage()
                    {
                        FoliageName = request.Foliage.FoliageName,
                        FoliageTypeId = request.Foliage.FoliageTypeId,
                        FoliageNameId = request.Foliage.FoliageNameId,
                        FoliageSize = request.Foliage.FoliageSize 
                    };

                    dbContext.Foliage.Add(f);

                    Inventory i = new Inventory()
                    {
                        InventoryName = request.Inventory.InventoryName,
                        InventoryTypeId = request.Inventory.InventoryTypeId,
                        ServiceCodeId = request.Inventory.ServiceCodeId
                    };

                    dbContext.Inventory.Add(i);
                    dbContext.SaveChanges();

                    InventoryFoliageMap invFoliageMap = new InventoryFoliageMap();
                    invFoliageMap.InventoryId = i.InventoryId;
                    invFoliageMap.FoliageId = f.FoliageId;

                    dbContext.InventoryFoliageMap.Add(invFoliageMap);

                    if (request.ImageId > 0)
                    {
                        InventoryImageMap iImgMap = new InventoryImageMap();
                        iImgMap.InventoryId = i.InventoryId;
                        iImgMap.ImageId = request.ImageId;

                        dbContext.InventoryImageMap.Add(iImgMap);
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    foliage_id = f.FoliageId;
                }
            }
            catch (Exception ex)
            {

            }

            return foliage_id;
        }

        public long UpdateFoliage(ImportFoliageRequest request)
        {
            long foliageId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    if (request.AddFoliageRequest.Foliage.FoliageId != 0)
                    {
                        Foliage f = dbContext.Foliage.Where(a => a.FoliageId == request.AddFoliageRequest.Foliage.FoliageId).FirstOrDefault();

                        if (f.FoliageId > 0)
                        {
                            f.FoliageSize = request.AddFoliageRequest.Foliage.FoliageSize;
                        }
                    }

                    if (request.AddFoliageRequest.Inventory.InventoryId > 0)
                    {
                        ServiceCode code = dbContext.ServiceCode.Where(a => a.ServiceCodeId == request.AddFoliageRequest.Inventory.ServiceCodeId).FirstOrDefault();

                        if (code.ServiceCodeId > 0)
                        {
                            code.Cost = request.ServiceCode.Cost;
                            code.Description = request.ServiceCode.Description;
                            code.GeneralLedger = request.ServiceCode.GeneralLedger;
                            code.Price = request.ServiceCode.Price;
                            code.Taxable = request.ServiceCode.Taxable;
                        }
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }
            return foliageId;
        }

        public long ImportFoliage(ImportFoliageRequest request)
        {
            long foliageId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    long foliageTypeId = request.AddFoliageRequest.Foliage.FoliageTypeId;

                    if (!String.IsNullOrEmpty(request.FoliageType))
                    {
                        //add plant type
                        if (foliageTypeId == 0)
                        {
                            AddFoliageTypeRequest foliageTypeRequest = new AddFoliageTypeRequest();
                            foliageTypeRequest.FoliageTypeName = request.FoliageType;
                            foliageTypeId = AddFoliageType(foliageTypeRequest);
                        }

                        request.AddFoliageRequest.Foliage.FoliageTypeId = foliageTypeId;
                    }

                    string foliageTypeName = dbContext.FoliageType.Where(a => a.FoliageTypeId == foliageTypeId).Select(b => b.FoliageTypeName).First();

                    long foliageNameId = request.AddFoliageRequest.Foliage.FoliageNameId;

                    if (!String.IsNullOrEmpty(request.FoliageName))
                    {
                        //add foliage name
                        if (foliageNameId == 0)
                        {
                            AddFoliageNameRequest foliageNameRequest = new AddFoliageNameRequest();
                            foliageNameRequest.FoliageName = request.FoliageName;
                            foliageNameRequest.FoliageTypeId = foliageTypeId;
                            foliageNameId = AddFoliageName(foliageNameRequest);
                        }

                        request.AddFoliageRequest.Foliage.FoliageNameId = foliageNameId;
                    }

                    string foliageName = dbContext.FoliageName.Where(a => a.FoliageNameId == foliageNameId).Select(b => b.Name).First();

                    long serviceCodeId = request.ServiceCode.ServiceCodeId;
                    string serviceCodeName = request.ServiceCode.ServiceCode;

                    if (serviceCodeId == 0)
                    {
                        serviceCodeId = AddServiceCode(request.ServiceCode);
                    }

                    ServiceCode svcCode = dbContext.ServiceCode.Where(a => a.ServiceCodeId == serviceCodeId).First();

                    serviceCodeName = svcCode.ServiceCode1;


                    request.AddFoliageRequest.Foliage.FoliageName = foliageTypeName + "-" + foliageName;
                    request.AddFoliageRequest.Foliage.FoliageNameId = foliageNameId;
                    request.AddFoliageRequest.Foliage.FoliageSize = request.AddFoliageRequest.Foliage.FoliageSize;
                    request.AddFoliageRequest.Foliage.FoliageTypeId = foliageTypeId;
                    request.AddFoliageRequest.Foliage.FoliageTypeName = foliageTypeName;

                    request.AddFoliageRequest.Inventory.InventoryTypeId = (long)Enums.InventoryType.Foliage;
                    request.AddFoliageRequest.Inventory.InventoryTypeName = "Foliage";
                    request.AddFoliageRequest.Inventory.InventoryName = foliageTypeName + "-" + foliageName;
                    request.AddFoliageRequest.Inventory.ServiceCodeId = serviceCodeId;
                    request.AddFoliageRequest.Inventory.ServiceCodeName = serviceCodeName;

                    long inventoryId = InventoryExists(request.AddFoliageRequest.Inventory);

                    long imageId = 0;

                    if (inventoryId > 0)
                    {
                        request.AddFoliageRequest.Inventory.InventoryId = inventoryId;

                        InventoryFoliageMap ipm = dbContext.InventoryFoliageMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if (ipm != null && ipm.InventoryFoliageMapId != 0)
                        {
                            request.AddFoliageRequest.Foliage.FoliageId = ipm.FoliageId;
                        }

                        InventoryImageMap iim = dbContext.InventoryImageMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if (iim != null && iim.InventoryImageMapId > 0)
                        {
                            if (iim.ImageId == 0)
                            {
                                if (request.imageBytes != null && request.imageBytes.Length > 0)
                                {
                                    //add image
                                    imageId = AddPlantImage(request.imageBytes);
                                }

                                request.AddFoliageRequest.ImageId = imageId;
                            }
                        }
                    }
                    else
                    {
                        if (request.imageBytes != null && request.imageBytes.Length > 0)
                        {
                            //add image
                            imageId = AddPlantImage(request.imageBytes);
                        }

                        request.AddFoliageRequest.ImageId = imageId;
                    }

                    if (inventoryId == 0)
                    {
                        foliageId = AddFoliage(request.AddFoliageRequest);
                    }
                    else
                    {
                        UpdateFoliage(request);
                    }

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return foliageId;
        }
        #endregion

        #region Materials
        public long MaterialExists(MaterialDTO materialDTO)
        {
            long materialId = 0;

            Material m = dbContext.Material.Where(a => a.MaterialName == materialDTO.MaterialName && a.MaterialTypeId == materialDTO.MaterialTypeId).FirstOrDefault();

            if (m != null && m.MaterialId > 0)
            {
                materialId = m.MaterialId;
            }

            return materialId;
        }

        public long MaterialNameExists(string materialName)
        {
            long materialNameId = 0;

            MaterialName matName = dbContext.MaterialName.Where(a => a.Name == materialName).FirstOrDefault();

            if (matName != null && matName.MaterialNameId > 0)
            {
                materialNameId = matName.MaterialNameId;
            }

            return materialNameId;
        }

        public long MaterialTypeExists(string typeName)
        {
            long materialTypeId = 0;

            MaterialType matType = dbContext.MaterialType.Where(a => a.MaterialTypeName == typeName).FirstOrDefault();

            if (matType != null && matType.MaterialTypeId > 0)
            {
                materialTypeId = matType.MaterialTypeId;
            }

            return materialTypeId;
        }

        public bool MaterialNameIsNotUnique(string materialName)
        {
            bool notUnique = false;

            Material m = dbContext.Material.Where(a => a.MaterialName == materialName).FirstOrDefault();

            if (m != null && m.MaterialId > 0)
            {
                notUnique = true;
            }

            return notUnique;
        }
        public long AddMaterialType(AddMaterialTypeRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    MaterialType materialType = new MaterialType()
                    {
                        MaterialTypeName = request.MaterialTypeName,
                    };

                    dbContext.MaterialType.Add(materialType);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = materialType.MaterialTypeId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }
        public long AddMaterialName(AddMaterialNameRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    MaterialName materialName = new MaterialName()
                    {
                        Name = request.MaterialName,
                        MaterialTypeId = request.MaterialTypeId
                    };

                    dbContext.MaterialName.Add(materialName);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = materialName.MaterialNameId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }
        public long AddMaterial(AddMaterialRequest request)
        {
            long material_id = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    Material m = new Material()
                    {
                        MaterialName = request.Material.MaterialName,
                        MaterialTypeId = request.Material.MaterialTypeId,
                        MaterialNameId = request.Material.MaterialNameId,
                        MaterialSize = request.Material.MaterialSize
                    };

                    dbContext.Material.Add(m);

                    Inventory i = new Inventory()
                    {
                        InventoryName = request.Inventory.InventoryName,
                        InventoryTypeId = request.Inventory.InventoryTypeId,
                        ServiceCodeId = request.Inventory.ServiceCodeId
                    };

                    dbContext.Inventory.Add(i);

                    InventoryMaterialMap invMaterialMap = new InventoryMaterialMap();
                    invMaterialMap.InventoryId = i.InventoryId;
                    invMaterialMap.MaterialId = m.MaterialId;

                    dbContext.InventoryMaterialMap.Add(invMaterialMap);

                    if (request.ImageId > 0)
                    {
                        InventoryImageMap iImgMap = new InventoryImageMap();
                        iImgMap.InventoryId = i.InventoryId;
                        iImgMap.ImageId = request.ImageId;

                        dbContext.InventoryImageMap.Add(iImgMap);
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                    material_id = m.MaterialId;
                }
            }
            catch (Exception ex)
            {

            }

            return material_id;
        }
        public long UpdateMaterial(ImportMaterialRequest request)
        {
            long materialId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    if (request.AddMaterialRequest.Material.MaterialId != 0)
                    {
                        Material m = dbContext.Material.Where(a => a.MaterialId == request.AddMaterialRequest.Material.MaterialId).FirstOrDefault();

                        if (m != null && m.MaterialId > 0)
                        {
                            m.MaterialSize = request.AddMaterialRequest.Material.MaterialSize;
                        }
                    }

                    if (request.AddMaterialRequest.Inventory.InventoryId > 0)
                    {
                        ServiceCode code = dbContext.ServiceCode.Where(a => a.ServiceCodeId == request.AddMaterialRequest.Inventory.ServiceCodeId).FirstOrDefault();

                        if (code != null && code.ServiceCodeId > 0)
                        {
                            code.Cost = request.ServiceCode.Cost;
                            code.Description = request.ServiceCode.Description;
                            code.GeneralLedger = request.ServiceCode.GeneralLedger;
                            code.Price = request.ServiceCode.Price;
                            code.Taxable = request.ServiceCode.Taxable;
                        }
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return materialId;
        }
        public long ImportMaterial(ImportMaterialRequest request)
        {
            long materialId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    long materialTypeId = request.AddMaterialRequest.Material.MaterialTypeId;

                    if (!String.IsNullOrEmpty(request.MaterialType))
                    {
                        //add plant type
                        if (materialTypeId == 0)
                        {
                            AddMaterialTypeRequest materialTypeRequest = new AddMaterialTypeRequest();
                            materialTypeRequest.MaterialTypeName = request.MaterialType;
                            materialTypeId = AddMaterialType(materialTypeRequest);
                        }

                        request.AddMaterialRequest.Material.MaterialTypeId = materialTypeId;
                    }

                    string materialTypeName = dbContext.MaterialType.Where(a => a.MaterialTypeId == materialTypeId).Select(b => b.MaterialTypeName).First();

                    long materialNameId = request.AddMaterialRequest.Material.MaterialNameId;

                    if (!String.IsNullOrEmpty(request.MaterialName))
                    {
                        //add plant name
                        if (materialNameId == 0)
                        {
                            AddMaterialNameRequest materialNameRequest = new AddMaterialNameRequest();
                            materialNameRequest.MaterialName = request.MaterialName;
                            materialNameRequest.MaterialTypeId = materialTypeId;
                            materialNameId = AddMaterialName(materialNameRequest);
                        }

                        request.AddMaterialRequest.Material.MaterialNameId = materialNameId;
                    }

                    string materialName = dbContext.MaterialName.Where(a => a.MaterialNameId == materialNameId).Select(b => b.Name).First();

                    long serviceCodeId = request.ServiceCode.ServiceCodeId;
                    string serviceCodeName = request.ServiceCode.ServiceCode;

                    if (serviceCodeId == 0)
                    {
                        serviceCodeId = AddServiceCode(request.ServiceCode);
                    }

                    ServiceCode svcCode = dbContext.ServiceCode.Where(a => a.ServiceCodeId == serviceCodeId).First();

                    serviceCodeName = svcCode.ServiceCode1;


                    request.AddMaterialRequest.Material.MaterialName = materialTypeName + "-" + materialName;
                    request.AddMaterialRequest.Material.MaterialNameId = materialNameId;
                    request.AddMaterialRequest.Material.MaterialTypeId = materialTypeId;
                    request.AddMaterialRequest.Material.MaterialTypeName = materialTypeName;

                    request.AddMaterialRequest.Inventory.InventoryTypeId = (long)Enums.InventoryType.Materials;
                    request.AddMaterialRequest.Inventory.InventoryTypeName = "Materials";
                    request.AddMaterialRequest.Inventory.InventoryName = materialTypeName + "-" + materialName;
                    request.AddMaterialRequest.Inventory.ServiceCodeId = serviceCodeId;
                    request.AddMaterialRequest.Inventory.ServiceCodeName = serviceCodeName;

                    long inventoryId = InventoryExists(request.AddMaterialRequest.Inventory);

                    long imageId = 0;

                    if (inventoryId > 0)
                    {
                        request.AddMaterialRequest.Inventory.InventoryId = inventoryId;

                        InventoryMaterialMap ipm = dbContext.InventoryMaterialMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if (ipm != null && ipm.InventoryMaterialMapId != 0)
                        {
                            request.AddMaterialRequest.Material.MaterialId = ipm.MaterialId;
                        }

                        InventoryImageMap iim = dbContext.InventoryImageMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if (iim != null && iim.InventoryImageMapId > 0)
                        {
                            if (iim.ImageId == 0)
                            {
                                if (request.imageBytes != null && request.imageBytes.Length > 0)
                                {
                                    //add image
                                    imageId = AddPlantImage(request.imageBytes);
                                }

                                request.AddMaterialRequest.ImageId = imageId;
                            }
                        }
                    }
                    else
                    {
                        if (request.imageBytes != null && request.imageBytes.Length > 0)
                        {
                            //add image
                            imageId = AddPlantImage(request.imageBytes);
                        }

                        request.AddMaterialRequest.ImageId = imageId;
                    }

                    if (inventoryId == 0)
                    {
                        materialId = AddMaterial(request.AddMaterialRequest);
                    }
                    else
                    {
                        UpdateMaterial(request);
                    }

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return materialId;
        }
        #endregion

        #region Container
        public long ContainerExists(ContainerDTO containerDTO)
        {
            long containerId = 0;

            Container c = dbContext.Container.Where(a => a.ContainerName == containerDTO.ContainerName && a.ContainerTypeId == containerDTO.ContainerTypeId).FirstOrDefault();

            if (c != null && c.ContainerId > 0)
            {
                containerId = c.ContainerId;
            }

            return containerId;
        }

        public long ContainerNameExists(string containerName)
        {
            long containerNameId = 0;

            ContainerName conName = dbContext.ContainerName.Where(a => a.Name == containerName).FirstOrDefault();

            if (conName != null && conName.ContainerNameId > 0)
            {
                containerNameId = conName.ContainerNameId;
            }

            return containerNameId;
        }

        public long ContainerTypeExists(string typeName)
        {
            long containerTypeId = 0;

            ContainerType conType = dbContext.ContainerType.Where(a => a.ContainerTypeName == typeName).FirstOrDefault();

            if (conType != null && conType.ContainerTypeId > 0)
            {
                containerTypeId = conType.ContainerTypeId;
            }

            return containerTypeId;
        }

        public bool ContainerNameIsNotUnique(string containerName)
        {
            bool notUnique = false;

            Container c = dbContext.Container.Where(a => a.ContainerName == containerName).FirstOrDefault();

            if (c != null && c.ContainerId > 0)
            {
                notUnique = true;
            }

            return notUnique;
        }
        public long AddContainerType(AddContainerTypeRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    ContainerType containerType = new ContainerType()
                    {
                        ContainerTypeName = request.ContainerTypeName,
                    };

                    dbContext.ContainerType.Add(containerType);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = containerType.ContainerTypeId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }
        public long AddContainerName(AddContainerNameRequest request)
        {
            long newId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    ContainerName containerName = new ContainerName()
                    {
                        Name = request.ContainerName,
                        ContainerTypeId = request.ContainerTypeId
                    };

                    dbContext.ContainerName.Add(containerName);
                    dbContext.SaveChanges();
                    scope.Complete();

                    newId = containerName.ContainerNameId;
                }
            }
            catch (Exception ex)
            {

            }

            return newId;
        }

        //public long AddContainer(AddContainerRequest request)
        //{
        //    long container_id = 0;

        //    try
        //    {
        //        using (var scope = new TransactionScope(TransactionScopeOption.Required))
        //        {
        //            Container c = new Container()
        //            {
        //                ContainerName = request.Container.ContainerName,
        //                ContainerTypeId = request.Container.ContainerTypeId,
        //            };

        //            dbContext.Container.Add(c);

        //            Inventory i = new Inventory()
        //            {
        //                InventoryName = request.Inventory.InventoryName,
        //                InventoryTypeId = request.Inventory.InventoryTypeId,
        //                ServiceCodeId = request.Inventory.ServiceCodeId
        //            };

        //            dbContext.Inventory.Add(i);

        //            InventoryContainerMap invContainerMap = new InventoryContainerMap();
        //            invContainerMap.InventoryId = i.InventoryId;
        //            invContainerMap.ContainerId = c.ContainerId;

        //            dbContext.InventoryContainerMap.Add(invContainerMap);

        //            if (request.ImageId > 0)
        //            {
        //                InventoryImageMap iImgMap = new InventoryImageMap();
        //                iImgMap.InventoryId = i.InventoryId;
        //                iImgMap.ImageId = request.ImageId;

        //                dbContext.InventoryImageMap.Add(iImgMap);
        //            }

        //            dbContext.SaveChanges();
        //            scope.Complete();
        //            container_id = c.ContainerId;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return container_id;
        //}
        public long UpdateContainer(ImportContainerRequest request)
        {
            long containerId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    if (request.AddContainerRequest.Container.ContainerId != 0)
                    {
                        Container c = dbContext.Container.Where(a => a.ContainerId == request.AddContainerRequest.Container.ContainerId).FirstOrDefault();

                        if (c.ContainerId > 0)
                        {
                            c.ContainerSize = request.AddContainerRequest.Container.ContainerSize;
                        }
                    }

                    if (request.AddContainerRequest.Inventory.InventoryId > 0)
                    {
                        ServiceCode code = dbContext.ServiceCode.Where(a => a.ServiceCodeId == request.AddContainerRequest.Inventory.ServiceCodeId).FirstOrDefault();

                        if (code.ServiceCodeId > 0)
                        {
                            code.Cost = request.ServiceCode.Cost;
                            code.Description = request.ServiceCode.Description;
                            code.GeneralLedger = request.ServiceCode.GeneralLedger;
                            code.Price = request.ServiceCode.Price;
                            code.Taxable = request.ServiceCode.Taxable;
                        }
                    }

                    dbContext.SaveChanges();
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return containerId;
        }
        public long ImportContainer(ImportContainerRequest request)
        {
            long containerId = 0;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    long containerTypeId = request.AddContainerRequest.Container.ContainerTypeId;

                    if (!String.IsNullOrEmpty(request.ContainerType))
                    {
                        //add plant type
                        if (containerTypeId == 0)
                        {
                            AddContainerTypeRequest containerTypeRequest = new AddContainerTypeRequest();
                            containerTypeRequest.ContainerTypeName = request.ContainerType;
                            containerTypeId = AddContainerType(containerTypeRequest);
                        }

                        request.AddContainerRequest.Container.ContainerTypeId = containerTypeId;
                    }

                    string containerTypeName = dbContext.ContainerType.Where(a => a.ContainerTypeId == containerTypeId).Select(b => b.ContainerTypeName).First();

                    long containerNameId = request.AddContainerRequest.Container.ContainerNameId;

                    if (!String.IsNullOrEmpty(request.ContainerName))
                    {
                        //add plant name
                        if (containerNameId == 0)
                        {
                            AddContainerNameRequest containerNameRequest = new AddContainerNameRequest();
                            containerNameRequest.ContainerName = request.ContainerName;
                            containerNameRequest.ContainerTypeId = containerTypeId;
                            containerNameId = AddContainerName(containerNameRequest);
                        }

                        request.AddContainerRequest.Container.ContainerNameId = containerNameId;
                    }

                    string containerName = dbContext.ContainerName.Where(a => a.ContainerNameId == containerNameId).Select(b => b.Name).First();

                    long serviceCodeId = request.ServiceCode.ServiceCodeId;
                    string serviceCodeName = request.ServiceCode.ServiceCode;

                    if (serviceCodeId == 0)
                    {
                        serviceCodeId = AddServiceCode(request.ServiceCode);
                    }

                    ServiceCode svcCode = dbContext.ServiceCode.Where(a => a.ServiceCodeId == serviceCodeId).First();

                    serviceCodeName = svcCode.ServiceCode1;


                    request.AddContainerRequest.Container.ContainerName = containerTypeName + "-" + containerName;
                    request.AddContainerRequest.Container.ContainerNameId = containerNameId;
                    request.AddContainerRequest.Container.ContainerTypeId = containerTypeId;
                    request.AddContainerRequest.Container.ContainerTypeName = containerTypeName;
                    request.AddContainerRequest.Container.ContainerSize = request.ContainerSize;

                    request.AddContainerRequest.Inventory.InventoryTypeId = (long)Enums.InventoryType.Containers;
                    request.AddContainerRequest.Inventory.InventoryTypeName = "Containers";
                    request.AddContainerRequest.Inventory.InventoryName = containerTypeName + "-" + containerName;
                    request.AddContainerRequest.Inventory.ServiceCodeId = serviceCodeId;
                    request.AddContainerRequest.Inventory.ServiceCodeName = serviceCodeName;

                    long inventoryId = InventoryExists(request.AddContainerRequest.Inventory);

                    long imageId = 0;

                    if (inventoryId > 0)
                    {
                        request.AddContainerRequest.Inventory.InventoryId = inventoryId;

                        InventoryContainerMap ipm = dbContext.InventoryContainerMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if (ipm.InventoryContainerMapId != 0)
                        {
                            request.AddContainerRequest.Container.ContainerId = ipm.ContainerId;
                        }

                        InventoryImageMap iim = dbContext.InventoryImageMap.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

                        if (iim != null && iim.InventoryImageMapId > 0)
                        {
                            if (iim.ImageId == 0)
                            {
                                if (request.imageBytes != null && request.imageBytes.Length > 0)
                                {
                                    //add image
                                    imageId = AddPlantImage(request.imageBytes);
                                }

                                request.AddContainerRequest.ImageId = imageId;
                            }
                        }
                    }
                    else
                    {
                        if (request.imageBytes != null && request.imageBytes.Length > 0)
                        {
                            //add image
                            imageId = AddPlantImage(request.imageBytes);
                        }

                        request.AddContainerRequest.ImageId = imageId;
                    }

                    if (inventoryId == 0)
                    {
                        containerId = AddContainer(request.AddContainerRequest);
                    }
                    else
                    {
                        UpdateContainer(request);
                    }

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {

            }

            return containerId;
        }

        #endregion

        public List<string> GetSizesByInventoryType(long inventoryTypeId)
        {
            List<string> sizes = new List<string>();

            switch(inventoryTypeId)
            {
                case 1: //Orchids
                    sizes = dbContext.Plant.Select(a => a.PlantSize).Distinct().ToList();
                    break;

                case 2: //Containers
                    sizes = dbContext.Container.Select(a => a.ContainerSize).Distinct().ToList();
                    break;

                case 4: //Foliage
                    sizes = dbContext.Foliage.Select(a => a.FoliageSize).Distinct().ToList();
                    break;

                case 5: //Materials
                    sizes = dbContext.Material.Select(a => a.MaterialSize).Distinct().ToList();
                    break;
            }

            return sizes;
        }

        private PriceAndTax GetPriceofInventoryItem(long inventoryId)
        {
            Inventory i = dbContext.Inventory.Where(a => a.InventoryId == inventoryId).FirstOrDefault();

            decimal price = 0;
            decimal tax = 0;

            if (i != null && i.InventoryId > 0)
            {
                ServiceCode code = dbContext.ServiceCode.Where(a => a.ServiceCodeId == i.ServiceCodeId).FirstOrDefault();

                if (code != null && code.Price.HasValue)
                {
                    price = code.Price.Value;
                }

                if (code.Taxable)
                {
                    tax = price * 0.06M;
                }
            }

            return new PriceAndTax(price, tax);
        }

        public GetWorkOrderSalesDetailResponse GetWorkOrderDetail(WorkOrderResponse request)
        {
            GetWorkOrderSalesDetailResponse response = new GetWorkOrderSalesDetailResponse();

            try
            {
                decimal subTotal = 0.0M;
                decimal tax = 0.0M;
                foreach (WorkOrderInventoryMapDTO workOrderItem in request.WorkOrderList)
                {
                    Inventory i = dbContext.Inventory.Where(a => a.InventoryId == workOrderItem.InventoryId).FirstOrDefault();

                    if (i != null && i.InventoryId > 0)
                    {
                        PriceAndTax priceAndTax = GetPriceofInventoryItem(i.InventoryId);

                        response.SubTotal += priceAndTax.Price * workOrderItem.Quantity;

                        response.Tax += priceAndTax.Tax;
                    }

                }

                foreach(NotInInventoryDTO notInInventory in request.NotInInventory)
                {
                    subTotal += notInInventory.NotInInventoryPrice * notInInventory.NotInInventoryQuantity;
                    tax = subTotal * 0.06M;
                    response.SubTotal += subTotal;
                    response.Tax += tax;
                }

                foreach(GetArrangementResponse  a in request.Arrangements)
                {
                    foreach(ArrangementInventoryItemDTO  b in a.ArrangementList)
                    {
                        PriceAndTax priceAndTax = GetPriceofInventoryItem(b.InventoryId);

                        response.SubTotal += priceAndTax.Price * b.Quantity;

                        response.Tax += priceAndTax.Tax;
                    }

                    foreach(NotInInventoryDTO notInInventory in a.NotInInventory)
                    {
                        subTotal += notInInventory.NotInInventoryPrice * notInInventory.NotInInventoryQuantity;
                        tax = subTotal * 0.06M;
                        response.SubTotal += subTotal;
                        response.Tax += tax;
                    }
                }

                response.Total = response.SubTotal + response.Tax;
            }
            catch(Exception ex)
            {
                
            }

            return response;
        }

        public void LogError(ErrorLogRequest request)
        {
            try
            {
                dbContext.ErrorLog.Add(new ErrorLog()
                {
                    Message = request.ErrorLog.Message,
                    Payload = request.ErrorLog.Payload,
                    Date = DateTime.Now
                });

                dbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                //ironic
            }
        }
    }
}
