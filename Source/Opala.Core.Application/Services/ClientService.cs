﻿using Microsoft.Extensions.Logging;
using Opala.Core.Domain.Adapters;
using Opala.Core.Domain.Exceptions;
using Opala.Core.Domain.Models;
using Opala.Core.Domain.Repositories;
using Opala.Core.Domain.Services;
using Otc.Validations.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Opala.Core.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IEmailAdapter emailAdapter;
        private readonly IClientReadOnlyRepository clientReadOnlyRepository;
        private readonly IClientWriteOnlyRepository clientWriteOnlyRepository;
        private readonly ISubscriptionService subscriptionService;
        private readonly ILogger logger;
        private readonly ApplicationConfiguration applicationConfiguration;

        public ClientService(IEmailAdapter emailAdapter, IClientReadOnlyRepository clientReadOnlyRepository, IClientWriteOnlyRepository clientWriteOnlyRepository, ApplicationConfiguration applicationConfiguration, ISubscriptionService subscriptionService, ILoggerFactory loggerFactory)
        {
            this.emailAdapter = emailAdapter ?? throw new ArgumentNullException(nameof(emailAdapter));
            this.clientReadOnlyRepository = clientReadOnlyRepository ?? throw new ArgumentNullException(nameof(clientReadOnlyRepository));
            this.clientWriteOnlyRepository = clientWriteOnlyRepository ?? throw new ArgumentNullException(nameof(clientWriteOnlyRepository));
            this.applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
            this.subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            this.logger = loggerFactory?.CreateLogger<ClientService>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task<bool> ClientExistsAsync(Guid clientId)
        {
            var result = await clientReadOnlyRepository.ClientExistsAsync(clientId);

            return result;
        }

        public async Task AddClientAsync(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            ValidationHelper.ThrowValidationExceptionIfNotValid(client);

            logger.LogInformation("Iniciando gravação do Cliente...");
            await clientWriteOnlyRepository.AddClientAsync(client);
            logger.LogInformation("Cliente gravado.");

            //Todo Disable for integration tests
            //logger.LogInformation("Enviando email para o Cliente");
            //await emailAdapter.SendAsync(client.Email, applicationConfiguration.EmailFrom, "Cadastro realizado com sucesso", "Seu cadastro foi realizado com sucesso");
        }

        public async Task<Client> GetClientAsync(Guid clientId)
        {
            var client = await clientReadOnlyRepository.GetClientAsync(clientId);

            return client;
        }

        public async Task EnableDisableClientAsync(Guid clientId, bool isActive)
        {
            if (!await ClientExistsAsync(clientId))
                throw new ClientCoreException(ClientCoreError.ClientNotFound);

            await clientWriteOnlyRepository.EnableDisableClientAsync(clientId, isActive);
        }

        public async Task UpdateClientAsync(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            ValidationHelper.ThrowValidationExceptionIfNotValid(client);

            if (!await ClientExistsAsync(client.Id))
                throw new ClientCoreException(ClientCoreError.ClientNotFound);

            await clientWriteOnlyRepository.UpdateClientAsync(client);
        }

        public async Task RemoveClientAsync(Guid clientId)
        {
            if (!await ClientExistsAsync(clientId))
                throw new ClientCoreException(ClientCoreError.ClientNotFound);

            await clientWriteOnlyRepository.RemoveClientAsync(clientId);
        }

        public async Task<IEnumerable<Client>> GetClientsAsync()
        {
            var clients = await clientReadOnlyRepository.GetClientsAsync();

            return clients;
        }
    }
}
