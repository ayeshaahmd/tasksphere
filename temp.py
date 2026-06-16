import hashlib, hmac, base64
pw='Admin,012'
stored='PBKDF2$60000$OuNYpDH55gBwuWKBPFccsA==$rkDHf4IyKkw7IhKrkE1v22nhwdBnBNbOdY4lcTLfP14='
parts=stored.split('$')
iterations=int(parts[1])
salt=base64.b64decode(parts[2])
expected=base64.b64decode(parts[3])
actual=hashlib.pbkdf2_hmac('sha1', pw.encode('utf-8'), salt, iterations, dklen=len(expected))
print('match' if hmac.compare_digest(actual, expected) else 'nomatch')
