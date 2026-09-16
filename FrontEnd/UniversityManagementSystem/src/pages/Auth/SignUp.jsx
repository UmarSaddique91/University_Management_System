import React from 'react';
import LoginImg from '../../assets/LoginImg.png';
import { useNavigate } from 'react-router-dom';
import { UserPlus, Mail, Lock, Eye, EyeOff } from 'lucide-react';
import { useState } from 'react';
import axios from 'axios';

const SignUp = () => {
  const navigate = useNavigate();

  //State
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const [isError, setIsError] = useState(false);


  //Handle Signup

  const handleSignup = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post(

        "https://localhost:7272/api/auth/register",
        {
          email: email,
          password: password
        }



      );

      setMessage(response.data.message)

      setIsError(false)

      setTimeout(() => {
        navigate("/")
      }, 2000)

    } catch (error) {
      setMessage("Registration Failed", error)
      setIsError(true)

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
        <div className="bg-white border border-gray-100 rounded-3xl shadow-xl p-8 sm:p-10 w-full max-w-md">

          {/* UserPlus Icon Badge (Lucide) */}
          <div className="w-16 h-16 bg-blue-100/80 rounded-full flex items-center justify-center mx-auto mb-5">
            <UserPlus className="w-7 h-7 text-blue-600" />
          </div>

          {/* Heading & Subtitle */}
          <h2 className="text-2xl font-bold text-gray-900 text-center mb-2">
            Create Account
          </h2>
          <p className="text-sm text-gray-500 text-center leading-relaxed mb-6">
            Create your account to access the University Management System.
          </p>

          {/* Form */}
          <form onSubmit={handleSignup} className="space-y-4">

            {/* Email Address */}
            <div>
              <label htmlFor="email" className="block text-sm font-semibold text-gray-700 mb-1.5">
                Email Address
              </label>
              <div className="relative flex items-center">
                <Mail className="absolute left-3.5 w-4 h-4 text-gray-400 pointer-events-none" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  id="email"
                  placeholder="Enter your email"
                  className="w-full pl-10 pr-4 py-2.5 bg-white border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label htmlFor="password" className="block text-sm font-semibold text-gray-700 mb-1.5">
                Password
              </label>
              <div className="relative flex items-center">
                <Lock className="absolute left-3.5 w-4 h-4 text-gray-400 pointer-events-none" />
                <input
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  id="password"
                  placeholder="Create password"
                  className="w-full pl-10 pr-10 py-2.5 bg-white border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 cursor-pointer"
                >
                  {showPassword ? <Eye size={18} /> : <EyeOff size={18} />}
                </button>
              </div>
            </div>

            {/* Password Requirements Box */}
            <div className="bg-[#f8faff] border border-gray-100 rounded-xl p-3.5 text-xs text-gray-500 space-y-1">
              <p className="font-semibold text-gray-700">Your password must:</p>
              <ul className="list-disc list-inside space-y-0.5 text-gray-500 pl-1">
                <li>Be at least 6 characters long</li>
                <li>Include at least one uppercase letter</li>
                <li>Include at least one lowercase letter</li>
                <li>Include at least one number</li>
                <li>Include at least one special character</li>
              </ul>
            </div>

            {/* Create Account Button */}
            <div className="pt-2">
              <button
                type="submit"
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-3 px-4 rounded-xl flex items-center justify-center cursor-pointer shadow-sm transition duration-150"
              >
                Create Account
              </button>
            </div>

            {message && (

              <p
                className={`text-sm text-center mt-3 ${isError ? "text-red-500" : "text-green-600"
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

          {/* Already have an account? Sign In */}
          <div className="text-center">
            <p className="text-sm text-gray-500">
              Already have an account?{' '}
              <span
                onClick={() => navigate('/')}
                className="text-blue-600 font-semibold cursor-pointer hover:underline"
              >
                Sign In
              </span>
            </p>
          </div>

        </div>
      </div>

    </div>
  );
};

export default SignUp;

