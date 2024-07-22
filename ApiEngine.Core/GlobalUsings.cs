﻿global using ApiEngine.Core.Aop;
global using ApiEngine.Core.Cache;
global using ApiEngine.Core.Constants;
global using ApiEngine.Core.Database;
global using ApiEngine.Core.Extensions;
global using ApiEngine.Core.Handlers;
global using ApiEngine.Core.Job;
global using ApiEngine.Core.Options;
global using ApiEngine.Core.Providers;
global using AspNetCoreRateLimit;
global using Furion;
global using Furion.Authorization;
global using Furion.ConfigurableOptions;
global using Furion.DatabaseAccessor;
global using Furion.DataEncryption;
global using Furion.DataValidation;
global using Furion.DependencyInjection;
global using Furion.FriendlyException;
global using Furion.JsonSerialization;
global using Furion.LinqBuilder;
global using Furion.Logging.Extensions;
global using Furion.TimeCrontab;
global using Furion.UnifyResult;
global using Hangfire;
global using Hangfire.MemoryStorage;
global using LogDashboard;
global using LogDashboard.Models;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.AspNetCore.ResponseCompression;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;
global using NewLife.Caching;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Linq;
global using Newtonsoft.Json.Serialization;
global using Serilog;
global using Serilog.Events;
global using SqlSugar;
global using SqlSugar.Extensions;
global using System.ComponentModel;
global using System.Globalization;
global using System.IO.Compression;
global using System.Linq.Expressions;
global using System.Text;