using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Validation
{
    /// <summary>
    /// Resultado da validação
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };
        
        public static ValidationResult Failure(IEnumerable<string> errors) => new() 
        { 
            IsValid = false, 
            Errors = errors.ToList() 
        };
    }

    /// <summary>
    /// Serviço centralizado de validação
    /// </summary>
    public class ValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Valida um objeto usando o validador apropriado
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a validar</typeparam>
        /// <param name="instance">Instância a validar</param>
        /// <returns>Resultado da validação</returns>
        public async Task<ValidationResult> ValidateAsync<T>(T instance) where T : class
        {
            var validator = _serviceProvider.GetService(typeof(IValidator<T>)) as IValidator<T>;
            
            if (validator == null)
            {
                // Se não há validador específico, considera válido
                return ValidationResult.Success();
            }

            var result = await validator.ValidateAsync(instance);
            
            if (result.IsValid)
            {
                return ValidationResult.Success();
            }

            var errors = result.Errors.Select(e => e.ErrorMessage);
            return ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Valida um objeto de forma síncrona
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a validar</typeparam>
        /// <param name="instance">Instância a validar</param>
        /// <returns>Resultado da validação</returns>
        public ValidationResult Validate<T>(T instance) where T : class
        {
            var validator = _serviceProvider.GetService(typeof(IValidator<T>)) as IValidator<T>;
            
            if (validator == null)
            {
                return ValidationResult.Success();
            }

            var result = validator.Validate(instance);
            
            if (result.IsValid)
            {
                return ValidationResult.Success();
            }

            var errors = result.Errors.Select(e => e.ErrorMessage);
            return ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Valida e lança exceção se inválido
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a validar</typeparam>
        /// <param name="instance">Instância a validar</param>
        /// <exception cref="ValidationException">Lançada quando a validação falha</exception>
        public void ValidateAndThrow<T>(T instance) where T : class
        {
            var result = Validate(instance);
            
            if (!result.IsValid)
            {
                var errorMessage = string.Join("; ", result.Errors);
                throw new ValidationException(errorMessage);
            }
        }

        /// <summary>
        /// Valida múltiplos objetos
        /// </summary>
        /// <typeparam name="T">Tipo dos objetos a validar</typeparam>
        /// <param name="instances">Instâncias a validar</param>
        /// <returns>Dicionário com resultados de validação por índice</returns>
        public Dictionary<int, ValidationResult> ValidateMultiple<T>(IEnumerable<T> instances) where T : class
        {
            var results = new Dictionary<int, ValidationResult>();
            var index = 0;

            foreach (var instance in instances)
            {
                results[index] = Validate(instance);
                index++;
            }

            return results;
        }
    }

    /// <summary>
    /// Extensões para facilitar o uso do ValidationService
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Adiciona os validadores ao container de DI
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        /// <returns>Collection de serviços para chaining</returns>
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            // Registra os validadores
            services.AddScoped<IValidator<CreateHardwareTicketRequest>, CreateHardwareTicketValidator>();
            services.AddScoped<IValidator<CreateSoftwareTicketRequest>, CreateSoftwareTicketValidator>();
            services.AddScoped<IValidator<UpdateTicketStateRequest>, UpdateTicketStateValidator>();
            services.AddScoped<IValidator<DeleteTicketRequest>, DeleteTicketValidator>();
            services.AddScoped<IValidator<int>, IdValidator>();

            // Registra o serviço de validação
            services.AddScoped<ValidationService>();

            return services;
        }
    }
}