import React from 'react';
import LoginImg from '../../assets/LoginImg.png';
import { useNavigate } from 'react-router-dom';

const ForgotPassword = () => {
  const navigate = useNavigate();

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
      <div className="w-1/2 bg-white flex flex-col items-center justify-center shadow-lg py-8">
        <div className="bg-white border border-gray-200 rounded-lg shadow-lg p-8 w-full max-w-md overflow-hidden">
          <h2 className="text-2xl font-bold mb-2">Forgot Password?</h2>
          <p className="text-slate-500 text-sm">
            Enter your email address and we'll send you instructions to reset your password.
          </p>

          {/* Form */}
          <div className="mt-6">
            <form onSubmit={(e) => e.preventDefault()} className="space-y-4">

              {/* Email Address */}
              <div>
                <label htmlFor="email" className="font-semibold">Email Address</label>
                <input
                  type="email"
                  id="email"
                  placeholder="Enter your registered email"
                  className="w-full px-4 py-2 mt-2 border border-gray-800 hover:border-gray-300 rounded-md focus:outline-none"
                />
              </div>

              {/* Submit Button */}
              <button
                type="submit"
                className="w-full bg-blue-500 hover:bg-blue-600 text-center py-3 cursor-pointer rounded-lg mt-6 text-white font-medium"
              >
                Send Reset Link
              </button>

            </form>
          </div>

          {/* Back to Sign In */}
          <div className="text-center mt-6">
            <p className="text-sm text-slate-500">
              Remember your password?{' '}
              <span
                onClick={() => navigate('/')}
                className="text-blue-500 font-semibold cursor-pointer hover:underline"
              >
                Back to Sign In
              </span>
            </p>
          </div>

        </div>

        {/* Copyright & Links outside card */}
        <div className="text-center mt-6 text-xs text-slate-400 space-y-1">
          <div className="flex justify-center items-center gap-2">
            <span className="hover:underline cursor-pointer">Privacy Policy</span>
            <span>|</span>
            <span className="hover:underline cursor-pointer">Terms of Service</span>
            <span>|</span>
            <span className="hover:underline cursor-pointer">Contact Support</span>
          </div>
          <p>© 2026 University Management System. All rights reserved.</p>
        </div>

      </div>

    </div>
  );
};

export default ForgotPassword;

