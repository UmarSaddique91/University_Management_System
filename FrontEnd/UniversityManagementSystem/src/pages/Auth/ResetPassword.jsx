import React from 'react';
import LoginImg from '../../assets/LoginImg.png';
import { useNavigate,useSearchParams } from 'react-router-dom';
import { Lock, Eye, EyeOff, LockKeyhole } from 'lucide-react';
import { useState } from 'react';
import axios from 'axios';

const ResetPassword = () => {
  const navigate = useNavigate();

  const [searchParams]= useSearchParams();
  const email= searchParams.get("email");
  const token= searchParams.get("token")


  // STATES 
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('')


  const [showNewPassword, setShowNewPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  const [message, setMessage]=useState('')



const handleResetPassword =async (e) => {
  e.preventDefault();

  if (!newPassword || !confirmPassword) {
    setMessage("Please enter both password fields");
    return;
  }

  

  // Minimum 6 characters
  if (newPassword.length < 6) {
    setMessage("Password must be at least 6 characters long");
    return;
  }

  // At least one uppercase letter
  if (!/[A-Z]/.test(newPassword)) {
    setMessage("Password must include at least one uppercase letter");
    return;
  }

  // At least one lowercase letter
  if (!/[a-z]/.test(newPassword)) {
    setMessage("Password must include at least one lowercase letter");
    return;
  }

  // At least one number
  if (!/[0-9]/.test(newPassword)) {
    setMessage("Password must include at least one number");
    return;
  }

  // At least one special character
  if (!/[^a-zA-Z0-9]/.test(newPassword)) {
    setMessage("Password must include at least one special character");
    return;
  }


  //Both passwords should match

  if (newPassword !== confirmPassword) {
    setMessage("Passwords do not match");
    return;
  }



  try {
  const response = await axios.post(
    "https://localhost:7272/api/Auth/reset-password",
    {
      email: email,
      token: token,
      newPassword: newPassword
    }
  );

  setMessage(response.data.message);

} catch (error) {
  setMessage(
    error.response?.data?.message || "Password reset failed"
  );
}
};
  

  return (
    <div className="w-full min-h-screen bg-white flex">

      {/* Left Side Section */}
      <div className="w-1/2 bg-white md:flex items-center justify-center">
        <img
          src={LoginImg}
          alt="UMS Illustration"
          className="object-cover h-full"
        />
      </div>

      {/* Right Side Section */}
      <div className="w-1/2 bg-[#f8faff] flex flex-col items-center justify-center p-8">
        <div className="bg-white border border-gray-100 rounded-3xl shadow-xl p-8 sm:p-10 w-full max-w-md">

          {/* Lock Icon Badge */}
          <div className="w-16 h-16 bg-blue-100/80 rounded-full flex items-center justify-center mx-auto mb-5">
            <LockKeyhole className="w-7 h-7 text-blue-600" />
          </div>

          {/* Title & Subtitle */}
          <h2 className="text-2xl font-bold text-gray-900 text-center mb-2">
            Reset Password
          </h2>
          <p className="text-sm text-gray-500 text-center leading-relaxed mb-6">
            Enter your new password below to reset your account password.
          </p>

          {/* Form */}
          <form onSubmit={handleResetPassword} className="space-y-4">





            {/* New Password */}
            <div>
              <label htmlFor="new-password" className="block text-sm font-semibold text-gray-700 mb-1.5">
                New Password
              </label>
              <div className="relative flex items-center">
                <Lock className="absolute left-3.5 w-4 h-4 text-gray-400 pointer-events-none" />
                <input
                  type={showNewPassword ? "text" : "password"}
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  id="new-password"
                  placeholder="Enter new password"
                  className="w-full pl-10 pr-10 py-2.5 bg-white border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowNewPassword(!showNewPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 cursor-pointer"
                >
                  {showNewPassword ? <Eye size={18} /> : <EyeOff size={18} />}
                </button>
              </div>
            </div>

            {/* Password Requirements */}
            <div className="text-xs text-gray-500 space-y-1 pt-1">
              <p className="font-medium text-gray-600">Your password must:</p>
              <ul className="list-disc list-inside space-y-0.5 text-gray-500 pl-1">
                <li>Be at least 6 characters long</li>
                <li>Include at least one uppercase letter</li>
                <li>Include at least one lowercase letter</li>
                <li>Include at least one number</li>
                <li> Include at least one special character</li>
              </ul>
            </div>

            {/* Confirm New Password */}
            <div className="pt-2">
              <label htmlFor="confirm-password" className="block text-sm font-semibold text-gray-700 mb-1.5">
                Confirm New Password
              </label>
              <div className="relative flex items-center">
                <Lock className="absolute left-3.5 w-4 h-4 text-gray-400 pointer-events-none" />
                <input
                  type={showConfirmPassword ? "text" : "password"}
                  value={confirmPassword}
                  onChange={(e)=>setConfirmPassword(e.target.value)}
                  id="confirm-password"
                  placeholder="Confirm new password"
                  className="w-full pl-10 pr-10 py-2.5 bg-white border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 cursor-pointer"
                >
                  {showConfirmPassword ? <Eye size={18} /> : <EyeOff size={18} />}
                </button>
              </div>
            </div>

            
            {/* Submit Button */}
            <div className="pt-2">
              

              <button
                type="submit"
                
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-3 px-4 rounded-xl flex items-center justify-center cursor-pointer shadow-sm transition duration-150"
              >
                Reset Password
              </button>
            </div>

             {message && (
  <p className="text-sm text-center mt-3 text-red-500 font-medium">
    {message}
  </p>
)}

          </form>

          {/* Back to Login */}
          <div className="text-center mt-6">
            <button
              type="button"
              onClick={() => navigate('/')}
              className="text-sm text-blue-600 font-medium hover:underline cursor-pointer"
            >
              Back to Login
            </button>
          </div>

         

        </div>
      </div>

    </div>
  );
};

export default ResetPassword;

