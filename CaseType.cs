using System;
using System.Collections.Generic;
using DataGraph.Models;
using DataGraph.Models.EstateAdmin;
using DataGraph.Repository;
using GraphQL.DataLoader;
using GraphQL.Types;

namespace DataGraph.GraphQL.Types
{
    public class CaseType : ObjectGraphType<Case>
    {
        public CaseType(IDataLoaderContextAccessor ctx, ICaseRepository caseRepository,
            ILineRepository lineRepository,
            IAccountUserRepository accountUserRepository,
            IProductRepository productRepository
            )
        {
            Field(x => x.AccountUserId).Name("accountuserid").Description("User who created the case");
            Field(x => x.CaseFileId).Name("caseid").Description("case identifier");
            Field(x => x.CaseReference).Name("casereference").Description("Reference of the case");
            Field(x => x.CreatedOn).Name("datecreated").Description("Date the case was created");
            Field(x => x.ModifiedOn, nullable: true).Name("datemodified").Description("Modified on date");
            Field(x => x.CaseTypeId, type: typeof(IntGraphType)).Name("casetypeid").Description("Case Type Id");
            Field(x => x.MatterTypeId, type: typeof(IntGraphType)).Name("mattertypeid").Description("Matter Type Id");

            Field<ListGraphType<OrderType>, IEnumerable<Order>>()
                .Name("orders")
                .Argument<IntGraphType>("orderid", "")
                .ResolveAsync(context =>
                {
                    var orderId = context.GetArgument<int?>("orderid");

                    if (orderId != null)
                    {
                        return caseRepository.GetOrderById(orderId ?? 0);
                    }


                    var casefileid = context.Source.CaseFileId;
                    var loader = ctx.Context.GetOrAddBatchLoader<int, IEnumerable<Order>>("GetOrdersByCaseFileId",
                        caseRepository.GetOrdersByCaseIdsAsync);

                    return loader.LoadAsync(casefileid);
                });


            Field<AccountUserType, AccountUser>().Name("caseuser")
                .ResolveAsync(context =>
                {
                    var loader = ctx.Context.GetOrAddBatchLoader<int, AccountUser>("GetAccountUserById",
                        accountUserRepository.GetAccountUserById);

                    return loader.LoadAsync(context.Source.AccountUserId);
                });


            Field<ListGraphType<LineType>, IEnumerable<Line>>()
                .Name("lines")
                .Argument<StringGraphType>("lineGuid", "")
                .ResolveAsync(context =>
                {
                    var lineIdGuid = context.GetArgument<Guid>("lineGuid");

                    if (lineIdGuid != Guid.Empty)
                    {
                        return lineRepository.GetLineByLineIdGuid(lineIdGuid);
                    }

                    var casefileid = context.Source.CaseFileId;
                    var loader = ctx.Context.GetOrAddBatchLoader<int, IEnumerable<Line>>("GetLinesByCaseFileId",
                        lineRepository.GetLinesByCaseFileIds);
                    return loader.LoadAsync(casefileid);
                });
 
            Field<ListGraphType<CaseAddressType>, IEnumerable<CaseAddress>>()
                .Name("caseaddresses")
                .ResolveAsync(context =>
                {
                    var casefileid = context.Source.CaseFileId;
                    var loader = ctx.Context.GetOrAddBatchLoader<int, IEnumerable<CaseAddress>>(
                        "GetCaseAddressesByCaseFileId",
                        caseRepository.GetCaseAddressesByCaseFileIds);
                    return loader.LoadAsync(casefileid);
                });
            Field<ListGraphType<VoucherType>, IEnumerable<Voucher>>()
                .Name("casevouchers")
                .ResolveAsync(context =>
                {
                    var casefileid = context.Source.CaseFileId;
                    var loader = ctx.Context.GetOrAddBatchLoader<int, IEnumerable<Voucher>>(
                        "GetVouchersByCaseFileId",
                        caseRepository.GetVouchersByCaseFileIds);
                    return loader.LoadAsync(casefileid);
                });
            Field<ListGraphType<CaseDictionaryType>, IEnumerable<CaseDictionary>>()
                .Name("casedictionary")
                .ResolveAsync(context =>
                {
                    var casefileid = context.Source.CaseFileId;
                    var loader = ctx.Context.GetOrAddBatchLoader<int, IEnumerable<CaseDictionary>>(
                        "GetCaseDictionariesByCaseFileIds",
                        caseRepository.GetCaseDictionariesByCaseFileIds);
                    return loader.LoadAsync(casefileid);

                });

            Field<ListGraphType<CaseDocumentType>, IEnumerable<CaseDocument>>()
                .Name("casedocuments")
                .Argument<IntGraphType>("documentType", "")
                .Argument<IntGraphType>("documentId", "")
                .ResolveAsync(context =>
                {
                    var documentType = context.GetArgument<int?>("documentType", null);
                    var documentId = context.GetArgument<int>("documentId", 0);
                    var casefileid = context.Source.CaseFileId;
                    var loader = ctx.Context.GetOrAddBatchLoader<int, IEnumerable<CaseDocument>>(
                        "GetCaseDocuments", (ids) => caseRepository.GetCaseDocuments(ids, documentType, documentId));

                    return loader.LoadAsync(casefileid);
                });

        }
    }
}
