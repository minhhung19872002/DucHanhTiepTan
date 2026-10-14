-- PostgreSQL translation of the SQL Server views used by the original WinForms app.
CREATE EXTENSION IF NOT EXISTS unaccent;
-- Executed at API startup after migrations (idempotent).

CREATE OR REPLACE VIEW "TreatmentAdapter" AS
SELECT t0."PrescriptionId", t0."AppointmentNo", t0."SubId", t0."ServiceId", t0."LaboId", t0."OriginalPrice", t0."Price",
       t0."CreatedDate", t0."Paid", t0."DoctorName", t0."AssistantName", t0."Comment", t0."zQuantity", t0."ToothPositions",
       t0."PreviousDebt", t1."HasDone", t0."IsCancelled",
       (CASE WHEN t0."IsCancelled" THEN t0."PreviousDebt" - t0."Paid"
             ELSE (t0."PreviousDebt" + (t0."zQuantity"::bigint * t0."Price")) - t0."Paid" END) AS "Remain"
FROM "ContainedTreatment" t0
JOIN "Appointment" t1 ON t0."PrescriptionId" = t1."PrescriptionId" AND t0."AppointmentNo" = t1."No";

CREATE OR REPLACE VIEW "PrescriptionAdapter" AS
SELECT t2."Id" AS "BranchId", t2."Name" AS "BranchName", t0."Id", t0."CustomerId", t1."Name" AS "CustomerName",
       t1."Phone" AS "CustomerPhone", t0."HasDone", t0."Treatment", t0."Diagnostic", t0."Symptoms", t0."CreatedDate",
       t3."CreatedDate" AS "LastAppointmentDate", t3."AppointmentDate", t3."AppointmentReason", t3."Advised" AS "AppointmentAdvised",
       COALESCE((SELECT COUNT(*) FROM "Appointment" t5 WHERE t5."HasDone" AND t5."PrescriptionId" = t0."Id"), 0)::int AS "AppointmentNo",
       COALESCE((SELECT SUM(t6."Paid") FROM "TreatmentAdapter" t6 WHERE t6."PrescriptionId" = t0."Id" AND t6."HasDone"), 0)::bigint AS "Paid",
       COALESCE((SELECT t8."Remain" FROM "TreatmentAdapter" t8
                 WHERE t8."PrescriptionId" = t0."Id"
                   AND t8."SubId" = (SELECT MAX(t9."SubId") FROM "TreatmentAdapter" t9 WHERE t9."PrescriptionId" = t0."Id" AND t9."HasDone")
                 LIMIT 1), 0)::bigint AS "Remain",
       t1."VisiableId" AS "CustomerVisiableId"
FROM "Prescription" t0
JOIN "Customer" t1 ON t0."CustomerId" = t1."Id"
JOIN "Branch" t2 ON t1."BranchId" = t2."Id"
JOIN "Appointment" t3 ON t0."Id" = t3."PrescriptionId"
     AND t3."No" = (SELECT MAX(t4."No") FROM "Appointment" t4 WHERE t4."PrescriptionId" = t0."Id");

CREATE OR REPLACE VIEW "AppointmentAdapter" AS
SELECT t3."Id" AS "BranchId", t3."Name" AS "BranchName", t0."PrescriptionId", t0."No", t0."CreatedDate", t0."HasDone",
       t0."AppointmentDate", t0."AppointmentReason", t0."Advised", t2."Name" AS "CustomerName", t2."Phone" AS "CustomerPhone",
       t2."Id" AS "CustomerId", t5."FullName" AS "DoctorFullName",
       (SELECT t6."No" FROM "Appointment" t6 WHERE t6."PrescriptionId" = t0."PrescriptionId" AND t6."No" = t0."No" + 1) AS "NextAppointmentNo",
       COALESCE((SELECT SUM(t7."Paid") FROM "ContainedTreatment" t7 WHERE t7."PrescriptionId" = t0."PrescriptionId" AND t7."AppointmentNo" = t0."No"), 0)::bigint AS "Paid",
       t0."DoctorName", t2."VisiableId" AS "CustomerVisiableId"
FROM "Appointment" t0
JOIN "Prescription" t1 ON t0."PrescriptionId" = t1."Id"
JOIN "Customer" t2 ON t1."CustomerId" = t2."Id"
JOIN "Branch" t3 ON t2."BranchId" = t3."Id"
LEFT JOIN "Doctor" t4 ON t4."UserName" = t0."DoctorName"
LEFT JOIN "Employee" t5 ON t5."UserName" = t4."UserName";

CREATE OR REPLACE VIEW "EquipmentInvoiceView" AS
SELECT i."BranchId", b."Name" AS "BranchName", i."Id", i."CreatedDate",
       COALESCE((SELECT COUNT(*) FROM "ContainedEquipmentInvoice" c WHERE c."InvoiceId" = i."Id"), 0)::int AS "Quantity",
       COALESCE((SELECT SUM(c."Quantity" * c."Price") FROM "ContainedEquipmentInvoice" c WHERE c."InvoiceId" = i."Id"), 0)::bigint AS "Total"
FROM "EquipmentInvoice" i JOIN "Branch" b ON b."Id" = i."BranchId";

CREATE OR REPLACE VIEW "ExpenseView" AS
SELECT e."Id", e."CreatedDate", e."AccountantName", b."Name" AS "BranchName", a."FullName" AS "AccountantFullName",
       COALESCE((SELECT SUM(c."Amount") FROM "ContainedExpense" c WHERE c."ExpenseId" = e."Id"), 0)::bigint AS "Total", a."BranchId"
FROM "Expense" e JOIN "Account" a ON e."AccountantName" = a."UserName" JOIN "Branch" b ON a."BranchId" = b."Id";

CREATE OR REPLACE VIEW "ExportInvoiceView" AS
SELECT t0."Id", t0."CreatedDate", t0."BranchId",
       COALESCE((SELECT COUNT(*) FROM "ContainedEquipmentExportInvoice" c WHERE c."EquipmentExportInvoiceId" = t0."Id"), 0)::int AS "Quantity",
       b."Name" AS "BranchName"
FROM "EquipmentExportInvoice" t0 JOIN "Branch" b ON t0."BranchId" = b."Id";

CREATE OR REPLACE VIEW "InvoiceView" AS
SELECT b."Id" AS "BranchId", b."Name" AS "BranchName", i."Id", i."CreatedDate", i."UserName", a."FullName",
       COALESCE((SELECT COUNT(*) FROM "ContainedInvoice" c WHERE c."InvoiceId" = i."Id"), 0)::int AS "Quantity",
       COALESCE((SELECT SUM(c."Quantity" * c."Price") FROM "ContainedInvoice" c WHERE c."InvoiceId" = i."Id"), 0)::bigint AS "Total"
FROM "Invoice" i JOIN "Account" a ON a."UserName" = i."UserName" JOIN "Branch" b ON b."Id" = i."BranchId";
