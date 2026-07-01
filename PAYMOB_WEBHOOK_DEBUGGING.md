# Paymob Webhook - Debugging & Fix Guide

## 🔧 FIXES IMPLEMENTED

### 1. **CORS Middleware Order Fixed** ✅
**Problem:** You had two `UseCors()` calls:
```csharp
app.UseCors("AllowPaymob");      // Applied first
app.UseCors("AllowFrontend");    // OVERWRITES the first one
```
The second call overwrote the first, so Paymob's webhook was being rejected.

**Solution:** Implemented path-based CORS routing:
```csharp
app.Use(async (context, next) =>
{
    if (path.StartsWith("/api/paymob/webhook"))
    {
        // Allow any origin for webhook
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
    await next();
});
app.UseCors("AllowFrontend");  // Frontend still works normally
```

### 2. **Comprehensive Logging Added** ✅
The webhook now logs at every step:
- Whether the request body was received
- Transaction details (ID, success status, order IDs)
- HMAC verification status
- Order ID resolution
- Database update confirmation
- Any errors with full stack trace

**Check logs in Visual Studio Output window** to see exactly where it fails.

### 3. **HMAC Verification Enabled** ✅
Now verifies the webhook signature before processing:
```csharp
var verified = await _paymobService.VerifyWebhookAsync(transaction, hmac);
if (!verified)
{
    return Unauthorized("HMAC verification failed");
}
```

### 4. **Error Handling Added** ✅
Comprehensive try-catch with detailed error logging and appropriate HTTP responses.

---

## 🔍 HOW TO DEBUG THE WEBHOOK

### Step 1: Check the Output Window
1. Open **View → Output** in Visual Studio
2. Select **"Debug"** from the dropdown
3. Make a test payment on Paymob
4. Watch for these logs:
   - `========== WEBHOOK RECEIVED ==========` (webhook was called)
   - `✅ HMAC verified successfully` (signature check passed)
   - `✅ Payment status updated successfully` (database was updated)

### Step 2: Test with Webhook.site (Optional)
1. Go to https://webhook.site
2. Copy the unique URL
3. Set it in your `.env` as `PAYMOB_WEBHOOK_URL=https://webhook.site/your-unique-id`
4. Make a test payment
5. Watch in real-time what Paymob is sending

### Step 3: Check Database Directly
```sql
SELECT Id, PaymentStatus, PaymobTransactionId, PaymentDate 
FROM Orders 
WHERE Id = 'your-test-order-id'
```

---

## 📋 WHAT THE WEBHOOK DOES

1. **Receives** webhook from Paymob POST to `/api/paymob/webhook?hmac=xxx`
2. **Deserializes** the JSON body into `PaymobWebhookRequestDto`
3. **Verifies** the HMAC signature to ensure it's really from Paymob
4. **Extracts** the order ID from `merchant_order_id` or falls back to `order.id`
5. **Updates** the database:
   - Sets `Order.PaymentStatus = Paid (2)` or `Failed (3)`
   - Sets `Order.PaymobTransactionId = transaction.id`
   - Sets `Order.PaymentDate = DateTime.UtcNow`
   - Same updates for the `Payment` entity if it exists

---

## ⚙️ ENVIRONMENT VARIABLES TO VERIFY

Make sure these are in your `.env` file:

```env
PAYMOB_API_KEY=your_api_key
PAYMOB_SECRET_KEY=your_secret_key
PAYMOB_PUBLIC_KEY=your_public_key
PAYMOB_INTEGRATION_ID=1234567
PAYMOB_HMAC_SECRET=your_hmac_secret
PAYMOB_WEBHOOK_URL=https://yourdomain.com/api/paymob/webhook
PAYMOB_REDIRECTION_URL=https://yourdomain.com/payment-success
```

⚠️ **The `PAYMOB_HMAC_SECRET` must match what you configured in Paymob's dashboard!**

---

## 🧪 TESTING THE WEBHOOK

### Option A: Manual Test (GET endpoint)
```
GET /api/paymob/test-update/your-order-id?status=true
```
This works because you're not going through Paymob's API or CORS.

### Option B: Direct Database Update (to verify it works)
```sql
UPDATE Orders SET PaymentStatus = 2, PaymobTransactionId = 'test-123'
WHERE Id = 'your-order-id'
```
Then refresh your frontend - does it show "Paid"?

### Option C: Production Payment
1. Make a real payment through Paymob
2. Check **Output window** in Visual Studio for webhook logs
3. Check database to see if status changed
4. Refresh frontend to verify

---

## 🚨 IF STILL NOT WORKING

### Issue: "Webhook received but request.obj is NULL"
- The JSON from Paymob doesn't match your DTO structure
- Check Paymob's actual webhook payload
- Compare field names (case-sensitive!)

### Issue: "No logs appearing at all"
- Webhook isn't reaching your server
- Check firewall/Azure App Service network rules
- Verify `PAYMOB_WEBHOOK_URL` is correct in your dashboard
- Test with webhook.site first

### Issue: "HMAC verification failed"
- Your `PAYMOB_HMAC_SECRET` in `.env` doesn't match Paymob's dashboard
- Verify the exact value in Paymob's integration settings

### Issue: "Order not found"
- The `merchant_order_id` in Paymob's payload doesn't match your Order.Id
- Check how you're creating the payment intent - ensure `special_reference` = `order.Id`

---

## 📝 RELATED CODE

- **Controller:** `Foodics/Controllers/PaymobController.cs` (webhook endpoint)
- **Service:** `Foodics/Services/PaymobService.cs` (UpdatePaymentStatusAsync, VerifyWebhookAsync)
- **DTOs:** `Foodics/Dtos/Paymob/PaymobWebhookRequestDto.cs`, `PaymobWebhookDto.cs`
- **Models:** `Foodics/Models/Order.cs`, `Foodics/Models/Payment.cs`
- **Config:** `Foodics/Program.cs` (CORS setup)

---

## ✅ NEXT STEPS

1. **Deploy** these changes to production
2. **Test** a real payment and check the logs
3. **Verify** the database updated correctly
4. **Monitor** logs for any issues
5. If still issues, **export the logs** and share them
