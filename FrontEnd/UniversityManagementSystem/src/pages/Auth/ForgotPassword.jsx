import React from 'react';
import LoginImg from '../../assets/LoginImg.png';
import { useNavigate } from 'react-router-dom';
import { Mail, Send, ArrowLeft, Info,LockKeyhole } from 'lucide-react';
import axios from 'axios';
import { useState } from 'react';

const ForgotPassword = () => {
  const navigate = useNavigate();

  const [email,setEmail]= useState('');
  const [message,setMessage]= useState('');
  const [isError, setIsError]=useState('');

  const handleForgotPassword=async (e)=>{
    e.preventDefault();

    try {
      const response= await axios.post(
        "https://localhost:7272/api/Auth/forgot-password",
        {
          email:email
        }
      );

      setMessage(response.data.message);

      setIsError(false);

    } catch (error) {
      setMessage("Something went wrong. Please try again.");

      setIsError(true);
    }
  }

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
        <div className="bg-white border border-gray-100 rounded-3xl shadow-xl p-8 
        sm:p-10 w-full max-w-md">

          {/* Lock Icon Badge */}
          <div className="w-16 h-16 bg-blue-100/80 rounded-full flex items-center justify-center 
          mx-auto mb-5">
              <LockKeyhole className="w-7 h-7 text-blue-600" />

          </div>

          {/* Title & Description */}
          <h2 className="text-2xl font-bold text-gray-900 text-center mb-2">
            Forgot Password?
          </h2>
          <p className="text-sm text-gray-500 text-center leading-relaxed mb-6">
            No worries! Enter your registered email address and we'll send you a link to reset your password.
          </p>

          {/* Form */}
          <form onSubmit={handleForgotPassword} className="space-y-5">

            {/* Email Address */}
            <div>
              <label htmlFor="email" className="block text-sm font-semibold text-gray-700 mb-1.5">
                Email Address
              </label>
              <div className="relative flex items-center">
                <Mail className="absolute left-3.5 w-5 h-5 text-gray-400 pointer-events-none" />
                <input
                  type="email"
                  value={email}
                  onChange={(e)=>setEmail(e.target.value)}
                  id="email"
                  placeholder="Enter your email address"
                  className="w-full pl-11 pr-4 py-2.5 bg-white border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                />
              </div>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-3 px-4 rounded-xl flex items-center justify-center gap-2 cursor-pointer shadow-sm transition duration-150"
            >
              <Send className="w-4 h-4" />
              <span>Send Reset Link</span>
            </button>

            {message && (
  <p
    className={`text-sm text-center mt-3 ${
      isError ? "text-red-500" : "text-green-600"
    }`}
  >
    {message}
  </p>
)}

          </form>

          {/* OR Divider */}
          <div className="flex items-center my-5">
            <div className="flex-1 border-t border-gray-200"></div>
            <span className="px-3 text-xs text-gray-400 font-medium">OR</span>
            <div className="flex-1 border-t border-gray-200"></div>
          </div>

          {/* Back to Sign In Button */}
          <button
            type="button"
            onClick={() => navigate('/')}
            className="w-full bg-white hover:bg-gray-50 border border-gray-200 text-blue-600 font-medium py-2.5 px-4 rounded-xl flex items-center justify-center gap-2 cursor-pointer transition duration-150"
          >
            <ArrowLeft className="w-4 h-4 text-blue-600" />
            <span>Back to Sign In</span>
          </button>

          {/* Information Notice */}
          <div className="flex items-start gap-2 text-xs text-gray-500 mt-6 pt-1">
            <Info className="w-4 h-4 text-gray-400 shrink-0 mt-0.5" />
            <span>
              If an account exists for this email, you will receive further instructions shortly.
            </span>
          </div>

        </div>
      </div>

    </div>
  );
};

export default ForgotPassword;
