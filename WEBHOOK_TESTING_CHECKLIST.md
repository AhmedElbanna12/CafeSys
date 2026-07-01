# 🚀 QUICK START - WEBHOOK TESTING

## ✅ Build Status
- ✅ Build is successful - ready to deploy

## 📋 PRE-DEPLOYMENT CHECKLIST

### 1. Environment Variables (`.env` file)
Verify these are set correctly:

```env
✓ PAYMOB_API_KEY=your_actual_api_key
✓ PAYMOB_SECRET_KEY=your_actual_secret_key
✓ PAYMOB_PUBLIC_KEY=your_actual_public_key
✓ PAYMOB_INTEGRATION_ID=1234567
✓ PAYMOB_HMAC_SECRET=your_actual_hmac_secret
✓ PAYMOB_WEBHOOK_URL=https://yourdomain.com/api/paymob/webhook
✓ PAYMOB_REDIRECTION_URL=https://yourdomain.com/payment-success
```

⚠️ **CRITICAL:** The `PAYMOB_HMAC_SECRET` must match exactly what's in your Paymob dashboard!

### 2. Paymob Dashboard Configuration
Go to your Paymob integration settings and verify:

- ✓ Webhook URL is set to: `https://yourdomain.com/api/paymob/webhook`
- ✓ Webhook events include: "Paid" or "Transaction" events
- ✓ HMAC secret is configured and matches your `.env`

---

## 🧪 TESTING PROCEDURE

### Step 1: Local Testing (Optional)
```
1. Run app locally: F5 in Visual Studio
2. Open: View → Output → Select "Debug" tab
3. Make test payment on Paymob (use test card)
4. Look for logs starting with "========== WEBHOOK RECEIVED =========="
5. Verify database shows PaymentStatus = 2 (Paid)
```

### Step 2: Production Testing
```
1. Deploy changes to production
2. Make a test payment via Paymob
3. SSH into server or check production logs
4. Look for webhook logs
5. Check database directly:
   SELECT Id, PaymentStatus, PaymobTransactionId 
   FROM Orders 
   ORDER BY CreatedAt DESC LIMIT 10
```

### Step 3: Verify in Frontend
```
1. Complete a payment on Paymob
2. Return to your app
3. Order should show "Paid" status
4. Verify all details are correct
```

---

## 🔍 DEBUGGING - CHECK LOGS FIRST

### If webhook is NOT appearing in logs:

**Problem:** Webhook never reached your server

**Check:**
- [ ] Is `PAYMOB_WEBHOOK_URL` correct in dashboard?
- [ ] Is it actually pointing to your live server?
- [ ] Can Paymob reach your domain? (try `curl` from another machine)
- [ ] Is firewall blocking? (check Azure Network Security Group rules)
- [ ] Port 443 (HTTPS) open? (port 80 won't work for webhooks)

**Test with webhook.site:**
```
1. Go to https://webhook.site
2. Copy your unique URL: https://webhook.site/xxxxxxxx-xxxx-xxxx
3. Update PAYMOB_WEBHOOK_URL in Paymob dashboard
4. Make a test payment
5. Watch webhook.site - do you see the POST request?
6. If yes → problem is in your code/deserialization
7. If no → problem is Paymob configuration or network
```

### If "HMAC verification FAILED" appears in logs:

**Problem:** HMAC signature doesn't match

**Check:**
- [ ] `PAYMOB_HMAC_SECRET` in `.env` matches Paymob dashboard exactly
- [ ] No typos, spaces, or extra characters
- [ ] Try disabling HMAC verification temporarily (comment out lines 137-149 in PaymobController.cs) to isolate issue

### If "Order not found" appears:

**Problem:** merchant_order_id doesn't match your Order.Id

**Check:**
- [ ] When creating payment intent, is `special_reference` = order.Id?
- [ ] Check PaymobService.cs line ~113: `special_reference = order.Id.ToString()`
- [ ] Are Order IDs strings or GUIDs? Make sure conversion is correct

### If "Request.obj null" appears:

**Problem:** JSON deserialization failed

**Check:**
- [ ] Check Paymob's actual webhook JSON structure
- [ ] Compare with PaymobWebhookDto.cs properties
- [ ] Property names must match exactly (case-sensitive!)
- [ ] Use webhook.site to capture actual Paymob payload

---

## 📊 DATABASE VERIFICATION

After a successful payment, check the database:

```sql
-- Check if order was updated
SELECT 
    Id, 
    PaymentStatus, 
    PaymobTransactionId, 
    PaymentDate,
    CreatedAt
FROM Orders 
WHERE Id = 'your-order-id'
LIMIT 1;

-- PaymentStatus values:
-- 0 = Pending
-- 1 = Processing
-- 2 = Paid ✅ (this is what we want)
-- 3 = Failed
-- 4 = Refunded
```

---

## 🐛 LOG OUTPUT EXAMPLES

### ✅ Successful Webhook
```
========== WEBHOOK RECEIVED ==========
Request null: False
Request.obj null: False
HMAC Query param: abc123def456...
Transaction ID: 123456789
Transaction Success: True
Order ID: 987654321
Merchant Order ID: test-order-123
🔐 Verifying HMAC signature...
✅ HMAC verified successfully
📦 Processing order: test-order-123
💳 Payment Status: SUCCESS
✅ Payment status updated successfully for order: test-order-123
=====================================
```

### ❌ Failed - Webhook Not Received
```
[NO LOGS AT ALL]
→ Check CORS, webhook URL, network connectivity
```

### ❌ Failed - HMAC Mismatch
```
🔐 Verifying HMAC signature...
❌ HMAC verification FAILED for transaction 123456789
→ Check PAYMOB_HMAC_SECRET matches dashboard
```

### ❌ Failed - Order Not Found
```
❌ No order ID found in webhook
→ Check merchant_order_id in Paymob payload
→ Verify it matches an Order.Id in database
```

---

## 📞 GETTING HELP

If still having issues, gather this information:

1. **Output log** from the webhook attempt (copy-paste from Visual Studio Output window)
2. **Database query result** showing the Order record
3. **Paymob webhook payload** (from webhook.site if using it)
4. **Verification**: Does manual test endpoint work?
   ```
   GET /api/paymob/test-update/test-order-123
   ```

---

## ✨ SUCCESS INDICATORS

You know it's working when:
- ✅ Webhook logs appear in Output window
- ✅ Database shows PaymentStatus = 2 (Paid)
- ✅ PaymobTransactionId is populated (not NULL)
- ✅ PaymentDate is set to current timestamp
- ✅ Frontend shows "Payment Successful" or similar

---

## 🔐 SECURITY REMINDER

- ✅ HMAC signature is now verified - webhooks must be from Paymob
- ✅ AllowAnonymous only on webhook endpoint - other endpoints protected
- ✅ Proper error handling prevents information disclosure
- ✅ Logs redact sensitive data where applicable

---

**Last Updated:** After fix implementation
**Status:** ✅ Ready for testing
