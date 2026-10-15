 SRP Responsibilities

 1 WardBoard

 Keeps the bed and patient state.
 Calculates clinical acuity.
 Decides and stores pager alerts.
 Formats handoff notes and exports census CSV.

These have different owners: a clinical scoring rule can change without a CSV format or a nurse-facing note changing I separated them into AcuityCalculator, PagerLog, HandoffNoteWriter, and CensusCsvExporter.

 2 CheckoutBasket

 Stores cart lines and customer choices.
 Calculates totals and discounts.
 Reads marketing coupon text.
 Writes gift cards and creates payment authorization text.

 Marketing coupon rules and payment formatting should not force a change in cart storage. The calculations are in BasketPricingService and CouponParser the messages and payment stub have their own classes.

 3 SupportTicket

 Stores the ticket conversation.
 Finds priority from text.
 Applies SLA times.
 Writes public and internal messages.

Support keywords and SLA hours and communication wording change for different reasons I moved them to TicketPriorityCalculator, SlaPolicy, PublicReplyWriter, and EscalationMessageWriter.

 4 LoanDesk

 Stores the loan request values.
 Calculates risk and eligibility.
 Creates a document checklist.
 Writes the decision letter and exports the underwriter row.

Risk policy is not the same concern as compliance documents or a CSV export Each one now has a small class with one job.

 5 CourseEnrollmentDesk

 Stores course data and forwards registration actions.
 Manages seated students and the waitlist.
 Creates the welcome packet.
 Formats the tuition invoice line.

Enrollment capacity rules are separate from marketing text and finance output EnrollmentRoster,WelcomePacketWriter, and TuitionInvoiceWriter now handle these jobs.

 6 KitchenTicket

 Stores order items.
 Detects allergens.
 Estimates kitchen time.
 Renders the thermal ticket and chooses an expo lane.

Allergen dictionaries and printer layout do not change together The separate scanner, calculator, renderer, and lane decider keep those reasons to change apart.

 7 SubscriptionBilling

 Stores subscription period and failed payment count.
 Calculates proration.
 Generates invoice numbers.
 Writes dunning email and ledger output.

Finance math, invoice numbering, collection wording, and journal formats are independent They were split into four helper classes.

 8 WarehousePickList

 Stores warehouse needs.
 Allocates stock.
 Sorts a walking route.
 Creates picker instructions and WMS XML.

Allocation policy, route optimization, handheld wording, and WMS integration can all change separately, so they are now different classes.

 9 GradeBook

 Stores student scores.
 Calculates averages.
 Applies letter and honor-roll policies.
 Writes a transcript and exports CSV.

Academic policies should not be tied to document formatting GradeCalculator, LetterGradePolicy, HonorRollPolicy, TranscriptWriter, and GradeCsvExporter each have a focused job.

10 AppointmentDesk

 Stores bookings.
 Checks business hours and finds a slot.
 Writes ICS calendar data.
 Writes SMS reminders.

Opening-hours rules, calendar interoperability, and SMS wording are separate concerns The desk now delegates them to focused classes.
