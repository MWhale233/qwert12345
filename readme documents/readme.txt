The 2 files below are using git LFS tracking:

cucko@DESKTOP-17EH574 MINGW64 /d/Codes/qwert12345/qwert12345 (main)
$ git lfs ls-files
7abdcb4fbb * fresnel-vr/Assets/Packages/MathNet.Numerics.MKL.Win-x64.3.0.0/runtimes/win-x64/native/libMathNetNumericsMKL.dll
0a731917fe - fresnel-vr/Assets/VolumeTextures/NewGeneratedTexture3D.asset

Developers needs to initialize git LFS too and use it to pull from repo to local if neccessary.


Dark screen problem when using Meta Quest Link:

**Problem 2: VR screen goes black or stays on three dots after streaming connection**  
**Solution:** Disable the integrated graphics in **Control Panel → Device Manager → Display Adapters**.

---

**Problem 3: After streaming, clicking "Desktop" in the main menu shows a black screen, but the mouse is visible**  
**Solution:**  
1. Go to **Windows Settings → Display → Graphics Settings** (located at the bottom).  
2. Under **"Graphics performance preference"**, select **"Choose an app to set preference" → "Desktop app"**.  
3. Click **"Browse"** and add:  
   - `C:\Program Files\Oculus\Support\oculus-runtime\OVRServer_x64.exe`, then set it to **"Power saving"**.  
   - `C:\Program Files\Oculus\Support\oculus-client\OculusClient.exe`, then set it to **"Power saving"**.  
4. Click **"Save"**.

